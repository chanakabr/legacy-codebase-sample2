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
        stage("Trigger Release Candidate"){
            when { expression { params.TRIGGER_RC == true } }
            steps{
                build (
                    job: "OTT-BE-Create-Release-Candidate", 
                    wait: false,
                    parameters: [
                        [$class: 'StringParameterValue', name: 'BRANCH_NAME', value: "${BRANCH_NAME}"],
                    ]
                )
            }
        }
    }
    post {
        always {
            report()
        }
    }

   
}

 def report(){
        configFileProvider([configFile(fileId: 'cec5686d-4d84-418a-bb15-33c85c236ba0', targetLocation: 'ReportJobStatus.sh')]) {}
        def GIT_COMMIT = sh(label:"Obtain GIT Commit", script: "cd tvpapi_rest && git rev-parse HEAD", returnStdout: true).trim();
        def reportout = sh (script: "chmod +x ReportJobStatus.sh && ./ReportJobStatus.sh ${BRANCH_NAME} build ${env.BUILD_NUMBER} ${env.JOB_NAME} build ${currentBuild.currentResult} ${GIT_COMMIT} NA", returnStdout: true)
        echo "${reportout}"
    }