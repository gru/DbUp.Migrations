"Stopping nuget-server"
docker stop nuget-server
"Removing nuget-server"
docker rm nuget-server
"Run nuget-server"
docker run -d --name nuget-server -p 80:80 -e NUGET_API_KEY="112233" idoop/docker-nuget-server