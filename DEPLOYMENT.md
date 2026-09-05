# Deploying with GitHub Actions and Fly.io

This project uses GitHub Actions to build every pull request and deploy successful pushes to `main` to Fly.io.

1. Create a Fly.io account and install `flyctl`.
2. From the repository root, run `fly launch --no-deploy`. Choose a globally unique application name and a region. This updates `fly.toml`; keep its Docker configuration and port `8080` unchanged.
3. Create a deployment token with `fly tokens create deploy -x 999999h`.
4. In the GitHub repository, open **Settings → Secrets and variables → Actions** and add a repository secret named `FLY_API_TOKEN` with the generated token.
5. Push `fly.toml` and the workflow to `main`.

The workflow first restores and builds the solution. Only a successful build on `main` deploys to Fly.io. Before the token is configured, the deployment step is skipped while CI still runs.
