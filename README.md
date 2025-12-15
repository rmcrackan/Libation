# Libation: Liberate your Library

## [Download Libation](https://github.com/rmcrackan/Libation/releases/latest)

### If you found this useful, tell a friend. If you found this REALLY useful, you can click here to [PayPal.me](https://paypal.me/mcrackan?locale.x=en_us)
...or just tell more friends. As long as I'm maintaining this software, it will remain **free** and **open source**.



## Development
### Documentation

The documentation is built with [VitePress](https://vitepress.dev/). For more information, see the [VitePress documentation](https://vuejs.github.io/vitepress/v1/guide/getting-started).

### Prerequisites

- [Node.js](https://nodejs.org/) version 18 or higher

### Setup

Install dependencies:
```bash
npm install
```

### Development

Start the local dev server with hot reload:
```bash
npm run docs:dev
```

The site will be available at `http://localhost:5173`.

### Build

Build the static site for production:
```bash
npm run docs:build
```

Preview the production build locally:
```bash
npm run docs:preview
```

### Deployment

The built site is output to `Documentation/.vitepress/dist`. Deploy this directory to any static hosting service (GitHub Pages, Cloudflare Pages, Vercel, Netlify, etc.).

