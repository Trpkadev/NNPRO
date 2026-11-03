import { globalIgnores } from 'eslint/config'
import { defineConfigWithVueTs, vueTsConfigs } from '@vue/eslint-config-typescript'
import pluginVue from 'eslint-plugin-vue'
import pluginVitest from '@vitest/eslint-plugin'
import pluginOxlint from 'eslint-plugin-oxlint'
import skipFormatting from 'eslint-config-prettier/flat'

import fs from 'node:fs'

const autoImportPath = new URL('./.eslintrc-auto-import.json', import.meta.url)
const autoImportGlobals = fs.existsSync(autoImportPath)
  ? JSON.parse(fs.readFileSync(autoImportPath, 'utf-8'))
  : { globals: {} }

// To allow more languages other than `ts` in `.vue` files, uncomment the following lines:
// import { configureVueProject } from '@vue/eslint-config-typescript'
// configureVueProject({ scriptLangs: ['ts', 'tsx'] })
// More info at https://github.com/vuejs/eslint-config-typescript/#advanced-setup

export default defineConfigWithVueTs(
  {
    name: 'app/files-to-lint',
    files: ['**/*.{vue,ts,mts,tsx}'],
  },

  globalIgnores(['**/dist/**', '**/dist-ssr/**', '**/coverage/**']),

  {
    name: 'app/files-to-ignore',
    // Ignoruj samotné vygenerované .d.ts soubory a složky Capacitoru
    ignores: [
      '**/dist/**',
      '**/dist-ssr/**',
      '**/coverage/**',
      '**/android/**',
      '**/ios/**',
      'src/auto-imports.d.ts',
      'src/components.d.ts',
    ],
  },

  {
    name: 'app/auto-imports',
    languageOptions: {
      globals: {
        ...autoImportGlobals.globals,
      },
    },
  },

  ...pluginVue.configs['flat/essential'],
  vueTsConfigs.recommended,

  {
    ...pluginVitest.configs.recommended,
    files: ['src/**/__tests__/*'],
  },

  ...pluginOxlint.buildFromOxlintConfigFile('.oxlintrc.json'),

  skipFormatting,
)
