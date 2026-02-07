package main

import (
	"fmt"
	"log"
	"time"

	"github.com/glebarez/sqlite"
	"gorm.io/gorm"
	"gorm.io/gorm/logger"
)

type Author struct {
	ID   uint `gorm:"primaryKey"`
	Name string
}

type Post struct {
	ID       uint `gorm:"primaryKey"`
	Title    string
	AuthorID uint
	Author   Author
}

func setupDB() *gorm.DB {
	db, err := gorm.Open(sqlite.Open("nplusone.db"), &gorm.Config{
		Logger: logger.Default.LogMode(logger.Info), // This is important to show the running queries
	})
	if err != nil {
		log.Fatal(err)
	}

	db.AutoMigrate(&Author{}, &Post{})

	// Seed data if empty
	var count int64
	db.Model(&Author{}).Count(&count)
	if count == 0 {
		authors := []Author{{Name: "Alice"}, {Name: "Bob"}, {Name: "Charlie"}}
		db.Create(&authors)

		for i := 1; i <= 50; i++ {
			db.Create(&Post{
				Title:    fmt.Sprintf("Post #%d", i),
				AuthorID: uint((i % 3) + 1),
			})
		}
	}

	return db
}

func badImplementation(db *gorm.DB) {
	fmt.Println("\n--- RUNNING BAD IMPLEMENTATION (N+1) ---")
	start := time.Now()

	var posts []Post
	db.Find(&posts) // Query 1: Fetch all posts

	for i := range posts {
		// N Queries: Fetch author for each post separately
		db.Model(&posts[i]).Association("Author").Find(&posts[i].Author)
	}

	duration := time.Since(start)
	fmt.Printf("Fetched %d posts with authors in %v\n", len(posts), duration)
}

func goodImplementation(db *gorm.DB) {
	fmt.Println("\n--- RUNNING GOOD IMPLEMENTATION (Eager Loading) ---")
	start := time.Now()

	var posts []Post
	// 1 Query: Fetch all posts with their authors using Preload (JOIN or INNER SELECT)
	db.Preload("Author").Find(&posts)

	duration := time.Since(start)
	fmt.Printf("Fetched %d posts with authors in %v\n", len(posts), duration)
}

func main() {
	db := setupDB()

	// Run bad implementation
	badImplementation(db)

	// Run good implementation
	goodImplementation(db)
}
