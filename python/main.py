import time
from sqlalchemy import create_engine, Column, Integer, String, ForeignKey
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker, relationship, joinedload

Base = declarative_base()

class Author(Base):
    __tablename__ = 'authors'
    id = Column(Integer, primary_key=True)
    name = Column(String)
    posts = relationship("Post", back_populates="author")

class Post(Base):
    __tablename__ = 'posts'
    id = Column(Integer, primary_key=True)
    title = Column(String)
    author_id = Column(Integer, ForeignKey('authors.id'))
    author = relationship("Author", back_populates="posts")

# Setup Database
engine = create_engine('sqlite:///nplusone.db', echo=True) # echo=True will print all SQL queries
Session = sessionmaker(bind=engine)
Base.metadata.create_all(engine)

def seed_data():
    session = Session()
    if session.query(Author).count() == 0:
        authors = [Author(name="Alice"), Author(name="Bob"), Author(name="Charlie")]
        session.add_all(authors)
        session.commit()
        
        posts = [Post(title=f"Post #{i}", author_id=(i % 3) + 1) for i in range(1, 101)]
        session.add_all(posts)
        session.commit()
    session.close()

def bad_implementation():
    print("\n--- RUNNING BAD IMPLEMENTATION (N+1) ---")
    session = Session()
    start_time = time.time()
    
    posts = session.query(Post).all() # Query 1
    results = []
    for post in posts:
        # Accessing post.author will trigger a new query if not loaded beforehand (Lazy Loading)
        results.append({
            "title": post.title,
            "author": post.author.name # Triggers N queries
        })
    
    end_time = time.time()
    print(f"Fetched {len(posts)} posts in {end_time - start_time:.4f} seconds")
    session.close()

def good_implementation():
    print("\n--- RUNNING GOOD IMPLEMENTATION (joinedload) ---")
    session = Session()
    start_time = time.time()
    
    # Eager Loading using joinedload
    posts = session.query(Post).options(joinedload(Post.author)).all() # 1 Query JOIN
    
    results = []
    for post in posts:
        results.append({
            "title": post.title,
            "author": post.author.name # Does not trigger additional queries
        })
    
    end_time = time.time()
    print(f"Fetched {len(posts)} posts in {end_time - start_time:.4f} seconds")
    session.close()

if __name__ == "__main__":
    seed_data()
    bad_implementation()
    good_implementation()
