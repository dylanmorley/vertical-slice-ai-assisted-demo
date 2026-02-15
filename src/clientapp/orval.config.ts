import { defineConfig } from 'orval'

export default defineConfig({
  verticalSlice: {
    input: {
      target: '../VerticalSlice.Web.Api/openapi.json',
    },
    output: {
      target: './src/api/generated/client.ts',
      schemas: './src/api/generated/model',
      client: 'fetch',
      clean: true,
      // baseUrl is handled at runtime via customFetch
      override: {
        mutator: {
          path: './src/api/customFetch.ts',
          name: 'customFetch',
        },
      },
    },
    hooks: {
      afterAllFilesWrite: 'prettier --write',
    },
  },
})
