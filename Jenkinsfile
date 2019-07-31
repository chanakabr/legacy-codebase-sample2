pipeline {
    agent none 
    stages {
        stage('Build and push core dockers'){
             parallel {
                stage('Build and Push Windows Docker') {
                    agent { label 'Jenkins-Docker-Windows' } 
                    steps {
                        //bb06d436-4816-4746-9ff2-9679d2ea3e52 - ott-ci-cd credentials id
                        git(url: 'https://github.com/kaltura/Core.git', branch: "master", credentialsId: 'bb06d436-4816-4746-9ff2-9679d2ea3e52')
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
                        //bb06d436-4816-4746-9ff2-9679d2ea3e52 - ott-ci-cd credentials id
                        git(url: 'https://github.com/kaltura/Core.git', branch: "master", credentialsId: 'bb06d436-4816-4746-9ff2-9679d2ea3e52')
                        sh label: "Docker build core:$DOCKER_BUILD_TAG", script: "docker build -t core:linux-$DOCKER_BUILD_TAG -f NetCore.Dockerfile --build-arg BRANCH=$BRANCH_NAME --build-arg BITBUCKET_TOKEN=$USERNAME:$TOKEN ."
                        script {
                            docker.withRegistry("https://870777418594.dkr.ecr.eu-west-1.amazonaws.com", "ecr:eu-west-1:dev") {
                                docker.image("linux-$DOCKER_BUILD_TAG").push("linux-$BRANCH_NAME")
                            }
                        }
                    }
                }
             }
         }
    }
}