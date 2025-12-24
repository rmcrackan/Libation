# Website & Docs

This documentation is built with [VitePress](https://vitepress.dev/) and located in the `docs` directory. For more information like [markdown syntax](https://vitepress.dev/guide/markdown#advanced-configuration) and [routing](https://vitepress.dev/guide/routing) or other features, refer [VitePress documentation](https://vitepress.dev/guide).

### Prerequisites

- Node.js 18+

### Commands

```bash
# Install dependencies
npm install

# Start local dev server (http://localhost:5173)
npm run docs:dev

# Build for production (output: docs/.vitepress/dist)
npm run docs:build

# Preview production build
npm run docs:preview
```

### Note

New pages are automatically routed based on their folder structure (e.g., `docs/docs/index.md` maps to `/docs/index`). To add them to the sidebar, update the `sidebar` configuration in `.vitepress/config.js`.
