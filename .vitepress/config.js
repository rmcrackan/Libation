import { defineConfig } from "vitepress";

// https://vitepress.dev/reference/site-config
export default defineConfig({
  vite: {
    // esbuild 0.28+ will not downlevel destructuring for safari14 / chrome87.
    // Raise the target so docs:dev and docs:build do not need that transform.
    build: {
      target: "es2022",
    },
    optimizeDeps: {
      esbuildOptions: {
        target: "es2022",
      },
    },
    esbuild: {
      supported: {
        destructuring: true,
      },
    },
    // Avoid watching the C# solution / VS locks (EBUSY on .vsidx files).
    server: {
      watch: {
        ignored: ["**/Source/**", "**/.vs/**", "**/bin/**", "**/obj/**"],
      },
    },
  },
  title: "Libation",
  description:
    "Libation: Liberate your Library - A free application for downloading your Audible audiobooks",
  head: [["link", { rel: "icon", href: "/favicon.ico" }]],
  cleanUrls: true,
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    logo: {
      light: "/libation_logo_light.svg",
      dark: "/libation_logo_dark.svg",
    },

    footer: {
      message: "Released under the GPLv3 License",
    },

    editLink: {
      pattern: "https://github.com/rmcrackan/Libation/edit/master/:path",
    },

    lastUpdated: true,

    nav: [
      { text: "Getting Started", link: "/docs/getting-started" },
      { text: "Docs", link: "/docs/index" },
      {
        text: "Download",
        link: "https://github.com/rmcrackan/Libation/releases/latest",
      },
      {
        text: "Issues & Requests",
        link: "https://github.com/rmcrackan/Libation/issues",
      },
      { text: "Donate", link: "/donate" },
    ],
    sidebar: [
      {
        items: [
          { text: "Overview", link: "/docs/index" },
          { text: "Getting Started", link: "/docs/getting-started" },
          { text: "FAQ", link: "/docs/frequently-asked-questions" },
          {
            text: "Issues & Requests",
            link: "https://github.com/rmcrackan/Libation/issues",
          },
          { text: "Donate", link: "/donate" },
        ],
      },
      {
        text: "Installation",
        collapsed: false,
        items: [
          { text: "Linux", link: "/docs/installation/linux" },
          { text: "Mac", link: "/docs/installation/mac" },
          { text: "Docker", link: "/docs/installation/docker" },
        ],
      },
      {
        text: "Features",
        collapsed: false,
        items: [
          {
            text: "Audio File Formats",
            link: "/docs/features/audio-file-formats",
          },
          {
            text: "Audiobookshelf Auto-Upload",
            link: "/docs/features/audiobookshelf",
          },
          {
            text: "Daily Download Limit",
            link: "/docs/features/daily-download-limit",
          },
          { text: "Naming Templates", link: "/docs/features/naming-templates" },
          {
            text: "Parallel Downloads",
            link: "/docs/features/parallel-downloads",
          },
          {
            text: "Retrying Refused Downloads",
            link: "/docs/features/retrying-refused-downloads",
          },
          {
            text: "Searching & Filtering",
            link: "/docs/features/searching-and-filtering",
          },
          {
            text: "Easy guide to searching",
            link: "/docs/features/lucene",
          },
          { text: "Trash Bin", link: "/docs/features/trash-bin" },
        ],
      },
      {
        text: "Advanced",
        collapsed: false,
        items: [
          { text: "Advanced Topics", link: "/docs/advanced/advanced" },
          { text: "Command Line Interface", link: "/docs/advanced/command-line-interface" },
          { text: "Troubleshooting", link: "/docs/advanced/troubleshoot" },
          { text: "Spatial Audio & DRM", link: "/docs/advanced/spatial-audio" },
        ],
      },
      {
        text: "Development",
        collapsed: false,
        items: [
          {
            text: "Getting Started",
            link: "/docs/development/getting-started",
          },
          { text: "Contribute", link: "/docs/development/contribute" },
          { text: "Testing Changes", link: "/docs/development/testing" },
          { text: "Website & Docs", link: "/docs/development/website" },
          { text: "Linux Setup (Nix)", link: "/docs/development/nix-linux-setup" },
        ],
      },
    ],

    outline: {
      level: "deep",
    },

    socialLinks: [
      { icon: "github", link: "https://github.com/rmcrackan/Libation" },
    ],

    search: {
      provider: "local",
    },

    // Show the external-link arrow on outbound links in markdown (nav/sidebar already do).
    externalLinkIcon: true,
  },
});
