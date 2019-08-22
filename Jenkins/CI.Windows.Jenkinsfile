pipeline {
    agent {
        label 'Windows'
    }
    options {
        buildDiscarder(logRotator(numToKeepStr:'10'))
        skipDefaultCheckout true
    }
    environment {
        MSBUILD = tool name: 'default', type: 'hudson.plugins.msbuild.MsBuildInstallation'
    }
    parameters {
        string(name: 'BRANCH_NAME', defaultValue: 'master', description: 'Branch')
        booleanParam(name: 'TRIGGER_RC', defaultValue: true, description: 'Should trigger Release Candidate?')
    }
    stages {
        stage("Checkout"){
            steps{
                script { currentBuild.displayName = "#${BUILD_NUMBER}: ${BRANCH_NAME}" }
                dir('core'){ git(url: 'https://github.com/kaltura/Core.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }
                dir('tvpapi_rest') { git(url: 'https://github.com/kaltura/Phoenix.git', branch: "${BRANCH_NAME}", credentialsId: "github-ott-ci-cd") }
                dir('clients-generator'){ git(url: 'https://github.com/kaltura/clients-generator.git', credentialsId: "github-ott-ci-cd") }
            }
        }
        stage("Version Patch"){
            steps{
                dir("core"){ bat "sh DllVersioning.Core.sh ." }
                dir("tvpapi_rest") { 
                    bat "sh ../core/DllVersioning.Core.sh ."
                    powershell "../Core/DllVersioning.ps1 ."
                }
            }        
        }
        stage("Nuget Restore"){
            steps{
                dir("core"){ bat ("nuget restore") }
                dir("tvpapi_rest"){ bat ("nuget restore") }
            }        
        }
        stage("Clean"){
            steps{
                sh(label:"clean bin and obj folders", script:"find . -iname 'bin' -o -iname 'obj' | xargs rm -rf")
                sh(label:"clean published folder", script:"rm -rf ./published")
            }
        }
        stage("Build"){
            steps{
                
                dir("tvpapi_rest"){
                    bat (label:"Run MSBuild Phoenix" , script:"\"${MSBUILD}\" Phoenix.Legacy\\Phoenix.Legacy.csproj -m:4 -nr:False -t:Restore,Build,WebPublish"
                            + " -p:Configuration=Release"
                            + " -p:DeployOnBuild=True"
                            + " -p:WebPublishMethod=FileSystem"
                            + " -p:DeleteExistingFiles=True"
                            + " -p:publishUrl=\"${WORKSPACE}/published/kaltura_ott_api/"
                    )


                    bat (label:"Run MSBuild Config Validator" ,script:"\"${MSBUILD}\" ConfigurationValidator\\ConfigurationValidator.csproj -m:4 -nr:False -t:Restore,Build"
                            + " -p:Configuration=Release"
                            + " -p:DeleteExistingFiles=True"
                            + " -p:OutDir=\"${WORKSPACE}/published/configuration_validator/"
                    )

                    bat (label:"Run MSBuild Permission Deployer", script:"\"${MSBUILD}\" PermissionsExport\\PermissionsDeployment.csproj -m:4 -nr:False -t:Restore,Build"
                            + " -p:Configuration=Release"
                            + " -p:DeleteExistingFiles=True"
                            + " -p:OutDir=\"${WORKSPACE}/published/permissions/"
                    )
                }
            }        
        }
        stage("Generate KalturaClient.xml"){
            steps { 
                dir("tvpapi_rest/Generator"){
                    bat (label:"Build Generator", script:"\"${MSBUILD}\" -p:Configuration=Release -m:4 -nr:False -t:Restore,Build")
                    dir("bin/Release/netcoreapp2.0"){
                        bat (label:"Generate KalturaClient.xml", script:"dotnet Generator.dll")
                        bat (label:"Copy KalturaClient.xml to clientlib folder", script:"xcopy KalturaClient.xml ${WORKSPACE}\\published\\kaltura_ott_api\\clientlibs\\")
                    }
                }
            }
        }
        stage("Generate Kaltura Clients and Docs"){
            steps { 
                dir("clients-generator"){
                    bat ("php exec.php --dont-gzip -x${WORKSPACE}\\published\\kaltura_ott_api\\clientlibs\\KalturaClient.xml "
                        +"-tott ottTestme,testmeDoc,php5,php53,php5Zend,php4,csharp,ruby,java,android,python,objc,cli,node,ajax " 
                        +"${WORKSPACE}\\published\\kaltura_ott_api\\clientlibs\\"
                    )
                }
            }
        }
        stage("Zip and Publish"){
            environment {
                    RELEASE_FULL_VERSION = sh(label:"Extract Full Verion Tag", script: 'cd tvpapi_rest && ../Core/get-version-tag.sh', , returnStdout: true).trim()
                    RELEASE_MAIN_VERSION = sh(label:"Extract Main Version Tag", script: "echo $version | sed -e 's/\\.[0-9]*\$//g'", , returnStdout: true).trim();
            }
            steps{
                dir("published"){  
                    bat (label:"Zip Artifacts", script:"7z.exe a -r phoenix-windows-${BRANCH_NAME}.zip *")
                    sh (label:"upload to s3", script:"aws s3 cp phoenix-windows-${BRANCH_NAME}.zip s3://${S3_BUILD_BUCKET_NAME}/mediahub/${BRANCH_NAME}/build/phoenix-windows-${BRANCH_NAME}.zip")
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
}