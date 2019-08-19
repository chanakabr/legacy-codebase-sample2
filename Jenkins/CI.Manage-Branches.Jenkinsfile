node {
    properties([
        parameters([
            choice(name: 'Action', description: 'Action', choices: ['Create', 'Delete']),
            string(name: 'Source_Branch', description: '', defaultValue: 'master'),
            string(name: 'Destination_Branch', description: '', defaultValue: ''),
        ])
    ])

    if (params.Action == 'Create' && params.Destination_Branch?.isAllWhitespace()){
        error ("Create branch action requires destination to be non empty")
    }

    stage('Clean'){
        deleteDir();
    }

    def repoNames = ['Core','Phoenix','RemoteTasks','tvmapps','tvpapi','tvplibs','WS_Ingest','ott-celery-tasks', 'TvinciCommon', 'Adapters']
    
    def parallelStageMap = [:]
    for (repoName in repoNames){
        def stageName = "${params.Action} Branch:${Source_Branch} Repo:${repoName}";
        parallelStageMap[stageName] = generateCreateNewBranchStage(stageName, repoName)
    }

    parallel parallelStageMap
}

def generateCreateNewBranchStage(stageName, repoName) {
    return {
        stage("${stageName}") {
           createNewBranch(repoName) 
        }
    }
}

def createNewBranch(repoName){
    withCredentials([[$class: 'UsernamePasswordMultiBinding', credentialsId: '	github-ott-ci-cd', usernameVariable: 'GIT_USERNAME', passwordVariable: 'GIT_PASSWORD']]) {
        def encoded_password = java.net.URLEncoder.encode(env.GIT_PASSWORD, "UTF-8")
        def gitUrl = "https://${env.GIT_USERNAME}:${encoded_password}@github.com/kaltura/${repoName}.git"
        sh(label:"Clone Bare Repo Branch ${Source_Branch}", script:"git clone --no-checkout --depth=1 --branch=${Source_Branch} ${gitUrl}")
        dir("${repoName}"){
            switch (params.Action){
                case 'Create':
                    sh("git checkout -b ${Destination_Branch}")
                    sh("git push origin ${Destination_Branch}:${Destination_Branch}")
                break
                case 'Delete':
                    try {
                        sh("git push origin --delete ${Source_Branch}")
                    }
                    catch (Exception e){
                        echo("WARNNING: failed delete branch:[${Source_Branch}] in repository:[${repoName}]")
                    }
                break
            }
        }
    }
}

