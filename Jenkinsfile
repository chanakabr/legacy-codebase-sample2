pipeline {
    agent none 
    stages {
        stage('Build and push core dockers'){
             parallel {
                stage('Build and Push Windows Docker') {
                    agent { label 'Jenkins-Docker-Windows' } 
                    steps {
                        env.DOCKER_BUILD_TAG = UUID.randomUUID().toString()
                        sh label: "Docker build core:$DOCKER_BUILD_TAG", script: "docker build -t core:win-$DOCKER_BUILD_TAG -f NetCore.Dockerfile --build-arg BRANCH=$BRANCH_NAME --build-arg BITBUCKET_TOKEN=$USERNAME:$TOKEN ."

                        script {
                            docker.withRegistry("https://870777418594.dkr.ecr.eu-west-1.amazonaws.com", "ecr:eu-west-1:dev") {
                                docker.image("core:win-$DOCKER_BUILD_TAG").push("win-$BRANCH_NAME")
                            }
                        }
                    }
                }
                stage('Build and Push Linux Docker') {
                    agent { label 'Ubuntu' } 
                    steps {
                        sh label: "Docker build core:$DOCKER_BUILD_TAG", script: "docker build -t core:$DOCKER_BUILD_TAG -f NetCore.Dockerfile --build-arg BRANCH=$BRANCH_NAME --build-arg BITBUCKET_TOKEN=$USERNAME:$TOKEN ."
                        script {
                            docker.withRegistry("https://870777418594.dkr.ecr.eu-west-1.amazonaws.com", "ecr:eu-west-1:dev") {
                                docker.image("$DOCKER_BUILD_TAG").push("$BRANCH_NAME")
                            }
                        }
                    }
                }
             }
         }
    }
}