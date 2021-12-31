# deploy gh-pages
npm run docs:build
git add docs/.vuepress/dist
git commit -m "Initial dist subtree commit"
git subtree push --prefix docs/.vuepress/dist origin gh-pages

# npm publish
npm login
npm version patch
npm publish