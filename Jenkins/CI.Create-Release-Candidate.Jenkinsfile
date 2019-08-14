
node{
    def s3ListCommand = '''
        aws s3 ls s3://ott-be-builds/mediahub/${BRANCH_NAME}/build/ | awk '{print $4}' | grep -oE '(.*)(-)' | sed 's/-$//g'
    '''
    def s3CopyBuildToRcCommand = '''
        aws s3 sync s3://ott-be-builds/mediahub/${BRANCH_NAME}/build/ s3://ott-be-builds/mediahub/${BRANCH_NAME}/rc/
    '''
    def foundArtifacts = []
    def requiredArtifacts=['celery-tasks','phoenix-windows','remote-tasks-windows','tvm','tvpapi-windows','ws-ingest-windows']
    def missingArtifacts=requiredArtifacts.collect()

    stage('Collect Build Artifacts')
    {
        foundArtifacts = sh(label:"Get Current Artifacts from S3", script: s3ListCommand, returnStdout: true).split()
    }

    stage('Identify Missing Artifacts') {
        for(artifact in foundArtifacts) {
            println("found: [${artifact}]")
            missingArtifacts.remove(artifact)
        }

        echo("missingArtifacts: ${missingArtifacts}")
        echo("foundArtifacts: ${foundArtifacts}")
        echo("requiredArtifacts: ${requiredArtifacts}")
    }
    
    stage('Build Missing Artifacts'){
        if (missingArtifacts.isEmpty()){
            echo("notinght left to build, coping artifacts to release candidate folder")
            sh(label:"Sync S3 Release Candidate Folder", script: s3CopyBuildToRcCommand, returnStdout: true)
            currentBuild.result = 'SUCCESS'
            return
        }
    }
    
    def jobsToBuild = [:]
    for(missingArtifact in missingArtifacts){
        def jobName = getJobNameFromArtifactName(missingArtifact)
        jobsToBuild["Build ${missingArtifact}"] = generateStage("Build ${missingArtifact}", jobName)

        parallel jobsToBuild
    }

    
    sh(label:"Sync S3 Release Candidate Folder", script: s3CopyBuildToRcCommand, returnStdout: true)

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

