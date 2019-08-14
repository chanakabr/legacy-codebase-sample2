pipeline {
    agent none 
    parameters {
        booleanParam(name: 'Build_Phoenix', defaultValue: true, description: 'Build Phoenix')
        booleanParam(name: 'Build_Remote_Tasks', defaultValue: true, description: 'Build Remote Tasks')
    }
    stages {
        stage('Build and push core dockers'){
             parallel {
                stage('Build and Push Windows Docker') {
                    agent { label 'Jenkins-Docker-Windows' } 
                    environment{ DOCKER_BUILD_TAG = UUID.randomUUID().toString() }
                    steps {
                        sh label: "Docker build core:win-${DOCKER_BUILD_TAG}", script: "docker build -t core:win-${DOCKER_BUILD_TAG} --build-arg BRANCH=${BRANCH_NAME} ."

                        script {
                            docker.withRegistry("https://870777418594.dkr.ecr.eu-west-1.amazonaws.com", "ecr:eu-west-1:dev") {
                                docker.image("core:win-${DOCKER_BUILD_TAG}").push("win-${BRANCH_NAME}")
                            }
                        }
                    }
                }
                stage('Build and Push Linux Docker') {
                    agent { label 'Ubuntu' } 
                    environment{ DOCKER_BUILD_TAG = UUID.randomUUID().toString() }
                    steps {
                        sh label: "Docker build core:${DOCKER_BUILD_TAG}", script: "docker build -t core:${DOCKER_BUILD_TAG} -f NetCore.Dockerfile --build-arg BRANCH=${BRANCH_NAME} ."
                        script {
                            docker.withRegistry("https://870777418594.dkr.ecr.eu-west-1.amazonaws.com", "ecr:eu-west-1:dev") {
                                docker.image("core:${DOCKER_BUILD_TAG}").push("${BRANCH_NAME}")
                            }
                        }
                    }
                }
             }
        }
        stage('Build Phoenix'){
            when { expression { return params.Build_Phoenix == true; } }
            steps{
                build (job: "k8s-Docker-Phoenix/${BRANCH_NAME}")
            }
        }
        stage('Build Remote Tasks'){
            when { expression { return params.Build_Remote_Tasks == true; } }
            steps{
               build (job: "k8s-Docker-Remote-Task/${BRANCH_NAME}") 
            }
        }
    }
}