# Libation: Liberate your Library

## [Download Libation](https://github.com/rmcrackan/Libation/releases/latest)

### If you found this useful, tell a friend. If you found this REALLY useful, you can click here to [PayPal.me](https://paypal.me/mcrackan?locale.x=en_us)

...or just tell more friends. As long as I'm maintaining this software, it will remain **free** and **open source**.

## Getting started with Libation

All documentation has been moved to our new site: [getlibation.com](https://getlibation.com). Or jump to the important bits:

* [Getting Started](https://getlibation.com/docs/getting-started)
* [Download](https://github.com/rmcrackan/Libation/releases/latest)
* [Issues, bugs, and requests](https://github.com/rmcrackan/Libation/issues)
* [Documentation](https://getlibation.com/docs/index)

## Development

### Documentation

The documentation is built with [VitePress](https://vitepress.dev/) and located in the `docs` directory. For more information like [markdown syntax](https://vitepress.dev/guide/markdown#advanced-configuration) and [routing](https://vitepress.dev/guide/routing) or other features, refer [VitePress documentation](https://vitepress.dev/guide).

**Prerequisites**: Node.js 18+

**Commands**:

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

**Note**: New pages are automatically routed based on their folder structure (e.g., `docs/docs/index.md` maps to `/docs/index`). To add them to the sidebar, update the `sidebar` configuration in `.vitepress/config.js`.
