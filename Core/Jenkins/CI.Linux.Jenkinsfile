def GIT_COMMIT=""
pipeline {
    agent {
        label 'Linux'
    }
    options {
        buildDiscarder(logRotator(numToKeepStr:'50'))
        skipDefaultCheckout true
    }
    parameters{
        string(name: 'BRANCH_NAME', defaultValue: 'master', description: 'Branch Name')
    }
    environment{
        AWS_REGION="us-west-2"
        REPOSITORY_NAME="${BRANCH_NAME.toLowerCase()}/core"
        ECR_REPOSITORY="870777418594.dkr.ecr.us-west-2.amazonaws.com/${REPOSITORY_NAME}"
    }
    stages {
        stage('Checkout'){
            steps{
                script { currentBuild.displayName = "#${BUILD_NUMBER}: ${BRANCH_NAME}" }
                git(url: 'https://github.com/kaltura/Core.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd")
                script{ GIT_COMMIT = sh(returnStdout: true, script: 'git rev-parse HEAD').trim() }
            }
        }
        stage('Build Docker'){
            environment{
                FULL_VERSION = sh(script: './get-version-tag.sh', , returnStdout: true).trim()
            }
            steps{
                sh(
                    label: "Docker build core:${BRANCH_NAME.toLowerCase()}", 
                    script: "docker build -t ${ECR_REPOSITORY}:build  "+
                    "-t ${ECR_REPOSITORY}:${GIT_COMMIT} "+
                    "-f NetCore.Dockerfile "+
                    "--build-arg BRANCH=${BRANCH_NAME} "+
                    "--label 'version=${FULL_VERSION}' "+
                    "--label 'commit=${GIT_COMMIT}' "+
                    "--label 'build=${env.BUILD_NUMBER}' ."
                )
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
        stage('Run Parallel Builds') {
            parallel {
                stage("Build Phoenix"){
                    steps{
                        build (job: "OTT-BE-Phoenix-Linux", parameters: [
                            [$class: 'StringParameterValue', name: 'BRANCH_NAME', value: "${BRANCH_NAME}"],
                        ]) 
                    }
                }

                stage("Build TVPAPI"){
                    steps{
                        build (job: "OTT-BE-Tvpapi-Linux", parameters: [
                            [$class: 'StringParameterValue', name: 'BRANCH_NAME', value: "${BRANCH_NAME}"],
                        ]) 
                    }
                }

                stage("Build RemoteTasks"){
                    steps{
                        build (job: "OTT-BE-Remote-Tasks-Linux", parameters: [
                            [$class: 'StringParameterValue', name: 'BRANCH_NAME', value: "${BRANCH_NAME}"],
                        ]) 
                    }
                }
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
    def GIT_COMMIT = sh(label:"Obtain GIT Commit", script: "git rev-parse HEAD", returnStdout: true).trim();
    def reportout = sh (script: "chmod +x ReportJobStatus.sh && ./ReportJobStatus.sh ${BRANCH_NAME} build ${env.BUILD_NUMBER} ${env.JOB_NAME} build ${currentBuild.currentResult} ${GIT_COMMIT} NA", returnStdout: true)
    echo "${reportout}"
}