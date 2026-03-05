<script setup>
import DefaultTheme from 'vitepress/theme'
import { ClientOnly } from 'vitepress/components'
import DownloadSuggestionWidget from './DownloadSuggestionWidget.vue'
import { useData } from 'vitepress'

const { frontmatter } = useData()
const { Layout: DefaultLayout } = DefaultTheme
</script>

<template>
  <DefaultLayout>
    <template #home-hero-after>
      <ClientOnly>
        <DownloadSuggestionWidget v-if="frontmatter.layout === 'home'" />
      </ClientOnly>
    </template>
  </DefaultLayout>
</template>
