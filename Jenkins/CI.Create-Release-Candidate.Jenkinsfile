
node('Linux'){
    stage('Check Success Artifacts'){
        configFileProvider([configFile(fileId: '75a8f192-08a4-41a7-80d7-2e85023c07f8', targetLocation: 'RC-GetFailedBuilds.sh')]) {}
        configFileProvider([configFile(fileId: 'cec5686d-4d84-418a-bb15-33c85c236ba0', targetLocation: 'ReportJobStatus.sh')]) {}
        configFileProvider([configFile(fileId: 'd38c6b7b-b2ab-446b-b37f-d6064570d795', targetLocation: 'UpdateBuildStage.sh')]) {}
        def result = sh (script: "chmod +x RC-GetFailedBuilds.sh && ./RC-GetFailedBuilds.sh ${BRANCH_NAME} build", returnStatus: true)
        if (result == 1){
            echo "${result}"
            def report = sh (script: "chmod +x ReportJobStatus.sh && ./ReportJobStatus.sh ${BRANCH_NAME} rc ${env.BUILD_NUMBER} ${env.JOB_NAME} rc FAILURE ", returnStdout: true)
            echo "${report}"
            currentBuild.result = 'FAILURE'
            sh 'exit 1'   
        }
            
    }
    stage('Update Artifacts to Stage RC'){
        sh (script: "chmod +x UpdateBuildStage.sh && ./UpdateBuildStage.sh ${BRANCH_NAME} build rc")
    }
        
    def s3CopyBuildToRcCommand = "aws s3 sync s3://ott-be-builds/mediahub/${BRANCH_NAME}/build/ s3://ott-be-builds/mediahub/${BRANCH_NAME}/rc/"
    def missingArtifacts = []
      
    stage('Identify Missing Artifacts') {
        missingArtifacts = FindMissingArtifacts();
    
        // If no missing artifacts, stop the job now and return Success
        if (missingArtifacts.isEmpty()){
            stage('Sync S3 Release Candidate Folder'){
                echo("nothing left to build, copying artifacts to release candidate folder")
                sh(label:"Sync S3 Release Candidate Folder", script: s3CopyBuildToRcCommand, returnStdout: true)
            }
            currentBuild.result = 'SUCCESS'
            configFileProvider([configFile(fileId: 'cec5686d-4d84-418a-bb15-33c85c236ba0', targetLocation: 'ReportJobStatus.sh')]) {}
            def report = sh (script: "chmod +x ReportJobStatus.sh && ./ReportJobStatus.sh ${BRANCH_NAME} rc ${env.BUILD_NUMBER} ${env.JOB_NAME} rc SUCCESS", returnStdout: true)
            echo "${report}"

            stage('Trigger Wrapper'){
                TriggerWrapper()
            }
            sh 'exit 0'
        }
    }

    stage('Build Missing Artifacts'){
       def jobsToBuild = [:]
        for(missingArtifact in missingArtifacts){
            def jobName = getJobNameFromArtifactName(missingArtifact)
            jobsToBuild["Build ${missingArtifact}"] = generateStage("Build ${missingArtifact}", jobName)
        }

        parallel jobsToBuild
    }

    stage('Validate Artifacts Built'){
        missingArtifacts = FindMissingArtifacts();
        if (missingArtifacts.isEmpty()){
            echo("All missing artifacts were delivered, copying artifacts to release candidate folder")
        }
        else{
            error("Failed to build missing artifacts")
            // Report Failed RC
            configFileProvider([configFile(fileId: 'cec5686d-4d84-418a-bb15-33c85c236ba0', targetLocation: 'ReportJobStatus.sh')]) {}
            def report = sh (script: "chmod +x ReportJobStatus.sh && ./ReportJobStatus.sh ${BRANCH_NAME} rc ${env.BUILD_NUMBER} ${env.JOB_NAME} rc FAILURE", returnStdout: true)
            echo "${report}"
            sh 'exit 1'
        }
    }
    
    stage('Sync S3 Release Candidate Folder'){
        sh(label:"Sync S3 Release Candidate Folder", script: s3CopyBuildToRcCommand, returnStdout: true)
    }
    currentBuild.result = 'SUCCESS'
    
    // Report Success RC
    configFileProvider([configFile(fileId: 'cec5686d-4d84-418a-bb15-33c85c236ba0', targetLocation: 'ReportJobStatus.sh')]) {}
    def report = sh (script: "chmod +x ReportJobStatus.sh && ./ReportJobStatus.sh ${BRANCH_NAME} rc ${env.BUILD_NUMBER} ${env.JOB_NAME} rc SUCCESS", returnStdout: true)
    echo "${report}"

    stage('Trigger Wrapper'){
        TriggerWrapper()
    }
}
    




def TriggerWrapper(){
    build job: 'OTT-BE-Test-Wrapper', parameters: [
                                                string(name: 'BRANCH', value: "${BRANCH_NAME}"),
                                                string(name: 'STAGE', value: "sanity"),
                                                string(name: 'AUTOKILL', value: "true"),
                                                ], wait: false
}
def FindMissingArtifacts(){
    def s3ListCommand = "aws s3 ls s3://ott-be-builds/mediahub/${BRANCH_NAME}/build/ | awk '{print \$4}' | grep -oE '(.*)(-)' | sed 's/-\$//g'"
    def foundArtifacts = []
    def requiredArtifacts = ['celery-tasks.zip','phoenix.zip','remote-tasks.zip','tvm.zip','tvpapi.zip','ws-ingest.zip']
    def missingArtifacts = requiredArtifacts.collect()

    foundArtifacts = sh(label:"Get Current Artifacts from S3", script: s3ListCommand, returnStdout: true).split()
    for(artifact in foundArtifacts) {
        println("found: [${artifact}]")
        missingArtifacts.remove(artifact)
    }

    echo("missingArtifacts: ${missingArtifacts}")
    echo("foundArtifacts: ${foundArtifacts}")
    echo("requiredArtifacts: ${requiredArtifacts}")
    return missingArtifacts
}

def generateStage(stageName, jobName) {
    return {
        stage("${stageName}") {
            build (
                job: "${jobName}", 
                wait: true,
                parameters: [
                    [$class: 'StringParameterValue', name: 'BRANCH_NAME', value: "${BRANCH_NAME}"],
                    [$class: 'BooleanParameterValue', name: 'TRIGGER_RC', value: false],
                ]
            )
        }
    }
}

def getJobNameFromArtifactName(repoName) {
    switch (repoName) {
        case 'phoenix-windows':
            return 'OTT-BE-Phoenix-Windows'
        case 'remote-tasks-windows':
            return 'OTT-BE-Remote-Tasks-Windows'
        case 'tvm':
            return 'OTT-BE-TVM'
        case 'tvpapi-windows':
            return 'OTT-BE-Tvpapi-Windows'
        case 'celery-tasks':
            return 'OTT-BE-Celery-Tasks'
        case 'ws-ingest-windows':
            return 'OTT-BE-WS-Ingest-Windows'
    }   
}

