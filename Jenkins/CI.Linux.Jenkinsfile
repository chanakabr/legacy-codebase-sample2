def GIT_COMMIT=""
def FULL_VERSION=""
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
        ECR_URL ='870777418594.dkr.ecr.us-west-2.amazonaws.com'
    }
    stages {
        stage('Checkout'){
            steps{
                script { currentBuild.displayName = "#${BUILD_NUMBER}: ${BRANCH_NAME}" }
                dir('core'){ git(url: 'https://github.com/kaltura/Core.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }
                dir('tvpapi_rest') { git(url: 'https://github.com/kaltura/Phoenix.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }

                script{
                    dir("tvpapi_rest"){ 
                        GIT_COMMIT = sh(returnStdout: true, script: 'git rev-parse HEAD').trim() 
                        FULL_VERSION = sh(script: '../core/get-version-tag.sh', , returnStdout: true).trim()
                    }
                }
            }
        }
        stage ('ECR Login') {
            steps{
                sh(label: "ECR Login", script: "login=\$(aws ecr get-login --no-include-email --region ${AWS_REGION}) && \${login}")
            }
        }
        stage('Build Phoenix Rest Docker'){
            environment{
                REPOSITORY_NAME="${BRANCH_NAME.toLowerCase()}/phoenix"
                ECR_REPOSITORY="${ECR_URL}/${REPOSITORY_NAME}"
                ECR_CORE_REPOSITORY="${ECR_URL}/${BRANCH_NAME.toLowerCase()}/core"
            }
            steps{
                dir("tvpapi_rest"){
                    sh(label: "ECR Login", script: "login=\$(aws ecr get-login --no-include-email --region ${AWS_REGION}) && \${login}")

                    sh(label: "Validate we have latest core docker image", script: "docker pull ${ECR_CORE_REPOSITORY}:build")
                    sh(
                        label: "Docker build ${REPOSITORY_NAME}", 
                        script: "docker build "+
                        "-t ${ECR_REPOSITORY}:build  "+
                        "-t ${ECR_REPOSITORY}:${GIT_COMMIT} "+
                        "--build-arg BRANCH=${BRANCH_NAME} "+
                        "--build-arg CORE_IMAGE=${ECR_CORE_REPOSITORY} "+
                        "--build-arg CORE_BUILD_TAG=build "+
                        "--label 'version=${FULL_VERSION}' "+
                        "--label 'commit=${GIT_COMMIT}' "+
                        "--label 'build=${env.BUILD_NUMBER}' ."
                    )
                }
                // Push to ecr - should be reusable. currently it is copy-paste so changes to this part should be applied on next stage as well
                sh(
                    label: "Verify ECR Repository Exist", 
                    script: "aws ecr describe-repositories --repository-names ${REPOSITORY_NAME} --region ${AWS_REGION} || "+
                            "aws ecr create-repository --repository-name ${REPOSITORY_NAME} --region ${AWS_REGION}"
                )
                sh(label: "Push Image", script: "docker push ${ECR_REPOSITORY}:build")
                sh(label: "Push Image", script: "docker push ${ECR_REPOSITORY}:${GIT_COMMIT}")
            }
        }
        stage('Build Phoenix Web Services Docker'){
            environment{
                REPOSITORY_NAME="${BRANCH_NAME.toLowerCase()}/phoenix-webservices"
                ECR_REPOSITORY="${ECR_URL}/${REPOSITORY_NAME}"
                ECR_CORE_REPOSITORY="${ECR_URL}/${BRANCH_NAME.toLowerCase()}/core"
            }
            steps{
                dir("tvpapi_rest"){
                    sh(label: "ECR Login", script: "login=\$(aws ecr get-login --no-include-email --region ${AWS_REGION}) && \${login}")

                    sh(label: "Validate we have latest core docker image", script: "docker pull ${ECR_CORE_REPOSITORY}:build")
                    sh(
                        label: "Docker build ${REPOSITORY_NAME}", 
                        script: "docker build "+
                        "-f WebServices.Dockerfile " +
                        "-t ${ECR_REPOSITORY}:build  "+
                        "-t ${ECR_REPOSITORY}:${GIT_COMMIT} "+
                        "--build-arg BRANCH=${BRANCH_NAME} "+
                        "--build-arg CORE_IMAGE=${ECR_CORE_REPOSITORY} "+
                        "--build-arg CORE_BUILD_TAG=build "+
                        "--label 'version=${FULL_VERSION}' "+
                        "--label 'commit=${GIT_COMMIT}' "+
                        "--label 'build=${env.BUILD_NUMBER}' ."
                    )
                }

                // Push to ecr - should be reusable. currently it is copy-paste so changes to this part should be applied on previous stage as well
                sh(
                    label: "Verify ECR Repository Exist", 
                    script: "aws ecr describe-repositories --repository-names ${REPOSITORY_NAME} --region ${AWS_REGION} || "+
                            "aws ecr create-repository --repository-name ${REPOSITORY_NAME} --region ${AWS_REGION}"
                )
                sh(label: "Push Image", script: "docker push ${ECR_REPOSITORY}:build")
                sh(label: "Push Image", script: "docker push ${ECR_REPOSITORY}:${GIT_COMMIT}")
            }
        }

        stage('Deploy to Linux Host'){
            steps{
                
                sh(label: "Run Image", script: "ssh 10.10.11.94 docker run -d -p 10.10.11.94:8080:80 --dns 10.10.11.94 -v /var/log:/var/log --name=phoenix-veon --log-opt max-size=150m -e TCM_URL='https://tcm.service.consul:8443' -e TCM_APP='OTT_API_SV_LINUX' -e TCM_SECTION='PROD_v5_2_2' -e TCM_APP_ID='5bf8cf60' -e TCM_APP_SECRET='5aaa99331c18f6bad4adeef93ab770c2' -e API_STD_OUT_LOG_LEVEL="Off" ${ECR_REPOSITORY}:${GIT_COMMIT}")
            }
        }
    }
}




// - add linux phoenix to chef
// - create a server with the right tags
// - make sure consul runs correctly 