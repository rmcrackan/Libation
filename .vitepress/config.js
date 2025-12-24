import { defineConfig } from "vitepress";
import { routex } from "@itznotabug/routex";

// https://vitepress.dev/reference/site-config
export default defineConfig({
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
      pattern: "https://github.com/rmcrackan/Libation/edit/main/:path",
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
      { text: "Donate", link: "https://www.paypal.com/paypalme/mcrackan" },
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
          { text: "Donate", link: "https://www.paypal.com/paypalme/mcrackan" },
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
          { text: "Naming Templates", link: "/docs/features/naming-templates" },
          {
            text: "Searching & Filtering",
            link: "/docs/features/searching-and-filtering",
          },
        ],
      },
      {
        text: "Advanced",
        collapsed: false,
        items: [{ text: "Advanced Topics", link: "/docs/advanced/advanced" }],
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
  },
  vite: {
    plugins: [
      routex({
        "/Documentation/Advanced.md": "/docs/advanced/advanced",
        "/Documentation/AudioFileFormats.md":
          "/docs/features/audio-file-formats",
        "/Documentation/Docker.md": "/docs/installation/docker",
        "/Documentation/FrequentlyAskedQuestions.md":
          "/docs/frequently-asked-questions",
        "/Documentation/GettingStarted.md": "/docs/getting-started",
        "/Documentation/InstallOnLinux.md": "/docs/installation/linux",
        "/Documentation/InstallOnMac.md": "/docs/installation/mac",
        "/Documentation/LinuxDevelopmentSetupUsingNix.md":
          "/docs/development/nix-linux-setup",
        "/Documentation/NamingTemplates.md": "/docs/features/naming-templates",
        "/Documentation/SearchingAndFiltering.md":
          "/docs/features/searching-and-filtering",
      }),
    ],
  },
});
