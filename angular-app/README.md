# Beers Angular workspace

Nx workspace for the Beers web application. The Angular 22 SSR app lives at
`apps/beers-web`; shared utilities, data access, and UI libraries are under
`libs/shared`.

## Start and verify

```sh
npm install
npm start
npm run build
npm run lint
npm test
```

Run the full target set with `npx nx run-many -t build,lint,test`.

## Architecture

Projects use `type:*` tags for layer rules and `scope:*` tags for domain rules.
Feature code may depend on UI, data access, and utility libraries. UI and data
access code may depend on utilities. All shared projects stay within the shared
scope; the app can depend on Beers and shared scope libraries.

The default shell uses the Fourteener pale theme. The theme toggle switches to
Fourteener stout and saves the choice in local storage. The theme tokens and
brand assets follow `../docs/spikes/834-color-scheme-app-icons.md`.
