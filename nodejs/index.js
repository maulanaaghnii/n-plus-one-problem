const { PrismaClient } = require('@prisma/client');
const express = require('express');
const app = express();
const prisma = new PrismaClient({
    log: ['query', 'info', 'warn', 'error'], // This will display SQL query logs in the console
});

async function seed() {
    const count = await prisma.author.count();
    if (count === 0) {
        await prisma.author.create({
            data: {
                name: 'Alice',
                posts: {
                    create: Array.from({ length: 50 }).map((_, i) => ({ title: `Post #${i + 1}` })),
                },
            },
        });
        console.log('Database seeded!');
    }
}

app.get('/bad', async (req, res) => {
    console.time('bad');
    const posts = await prisma.post.findMany(); // Query 1: Fetch all posts

    const results = [];
    for (const post of posts) {
        // N Queries: Fetch author for each post separately
        const author = await prisma.author.findUnique({
            where: { id: post.authorId },
        });
        results.push({ ...post, author });
    }
    console.timeEnd('bad');
    res.json({ count: results.length });
});

app.get('/good', async (req, res) => {
    console.time('good');
    // 1 Query using 'include' (similar to JOIN)
    const posts = await prisma.post.findMany({
        include: { author: true },
    });
    console.timeEnd('good');
    res.json({ count: posts.length });
});

const PORT = 3000;
seed().then(() => {
    app.listen(PORT, () => {
        console.log(`Server Node.js running on http://localhost:${PORT}`);
    });
});
