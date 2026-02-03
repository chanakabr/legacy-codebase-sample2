curl --location --request POST 'https://rest-sgs1.ott.kaltura.com/api_v3/service/asset/action/getPlaybackContext' --header 'Content-Type: application/json' --data-raw '{​​​​​​​
    apiVersion: 5.3.5,
    ks: djJ8MjI1fHwDg2d8XWe-Yb6-P0mi9QHh0SYdyp7iZAB6eHUA1sOuByIFMkNRBONw1A2hrgN7L93E-T6EbCNA8ianMkZT-e04wUGAlwkRSHH0ECFoQRp21bxcJnt7JR_uKLtsxIgDfuvNV3GwVO04HQehXT5A8BRrswINKrjWrZCVAxTT6W9QT3NMBn5HReXYLOs8OqxnHWUZblWalQQkEML0zScFqNTgbAUgElYsHcAbsJxDK3JIqhGxDlYFJq0HTMmoixKZ8mUJCF5Eu-kfTnj9AGWrAdB9Jx7-KjHqC1kWmKnsXu7bnfIel6dUJHBqkH8M5oZzf_2wBYREYU38_dV6ImC_zbA=,
    service: asset,
    action: getPlaybackContext,
    assetId: 337099,
     assetType: MEDIA,
    contextDataParams: {​​​​​​​
        objectType: KalturaPlaybackContextOptions,
        context: PLAYBACK}​​​​​​​
}​​​​​​​' -v  -i s/​//g
