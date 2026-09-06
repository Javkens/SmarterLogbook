# Deploying with GitHub Actions and Render

This project uses GitHub Actions for continuous integration and Render for deployment.

1. Push this repository to GitHub and keep `main` as the production branch.
2. In Render, choose **New → Blueprint**, connect the GitHub repository, and select the included `render.yaml` file.
3. Confirm the service name and create the Blueprint. Render creates a Docker web service in Frankfurt using the Free plan.
4. Push to `main`. GitHub Actions restores and builds the solution first. Render deploys only after that CI check succeeds.

The initial service URL uses the `onrender.com` domain. Free web services spin down after inactivity, so the first request after a pause can take longer to respond.
