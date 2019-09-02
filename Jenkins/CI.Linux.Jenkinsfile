def GIT_COMMIT=""
pipeline {
    agent {
        label 'Linux'
    }
    options {
        buildDiscarder(logRotator(numToKeepStr:'10'))
        skipDefaultCheckout true
    }
    parameters{
        string(name: 'BRANCH_NAME', defaultValue: 'master', description: 'Branch Name')
    }
    environment{
        AWS_REGION="us-west-2"
        BRANCH_NAME_TO_LOWER= sh (returnStdout: true, script: 'echo ${BRANCH_NAME} | tr '[:upper:]' '[:lower:]'').trim()
        REPOSITORY_NAME="${BRANCH_NAME_TO_LOWER}/phoenix"
        ECR_REPOSITORY="870777418594.dkr.ecr.us-west-2.amazonaws.com/${REPOSITORY_NAME}"
        ECR_CORE_REPOSITORY="870777418594.dkr.ecr.us-west-2.amazonaws.com/${BRANCH_NAME_TO_LOWER}/core"
    }
    stages {
        stage('Checkout'){
            steps{
                script { currentBuild.displayName = "#${BUILD_NUMBER}: ${BRANCH_NAME}" }
                dir('core'){ git(url: 'https://github.com/kaltura/Core.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }
                dir('tvpapi_rest') { git(url: 'https://github.com/kaltura/Phoenix.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }

                script{
                    dir("tvpapi_rest"){ GIT_COMMIT = sh(returnStdout: true, script: 'git rev-parse HEAD').trim() }
                }
            }
        }
        stage('Build Docker'){
            environment{
                FULL_VERSION = sh(script: 'cd tvpapi_rest && ../core/get-version-tag.sh', , returnStdout: true).trim()
            }
            steps{
                dir("tvpapi_rest"){
                    sh(label: "ECR Login", script: "login=\$(aws ecr get-login --no-include-email --region ${AWS_REGION}) && \${login}")

                    sh(label: "Validate we have latest core docker image", script: "docker pull ${ECR_CORE_REPOSITORY}:build")
                    sh(
                        label: "Docker build core:${BRANCH_NAME_TO_LOWER}", 
                        script: "docker build "+
                        "-t ${ECR_REPOSITORY}:build  "+
                        "-t ${ECR_REPOSITORY}:${GIT_COMMIT} "+
                        "--build-arg BRANCH=${BRANCH_NAME_TO_LOWER} "+
                        "--build-arg CORE_IMAGE=${ECR_CORE_REPOSITORY} "+
                        "--build-arg CORE_BUILD_TAG=build "+
                        "--label 'version=${FULL_VERSION}' "+
                        "--label 'commit=${GIT_COMMIT}' "+
                        "--label 'build=${env.BUILD_NUMBER}' ."
                    )
                }
            }
        }
        stage('Push to ECR'){
            steps{
                sh(label: "ECR Login", script: "login=\$(aws ecr get-login --no-include-email --region ${AWS_REGION}) && \${login}")
                sh(
                    label: "Verify ECR Repository Exist", 
                    script: "aws ecr describe-repositories --repository-names ${REPOSITORY_NAME} --region ${AWS_REGION} || "+
                            "aws ecr create-repository --repository-name ${REPOSITORY_NAME} --region ${AWS_REGION}"
                )
                sh(label: "Push Image", script: "docker push ${ECR_REPOSITORY}:build")
                sh(label: "Push Image", script: "docker push ${ECR_REPOSITORY}:${GIT_COMMIT}")
            }
        }
    }
}
