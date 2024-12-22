import { App } from './app-support.js'

App.init = async function () {
    App.Module.canvas = document.getElementById('canvas');
    await App.MONO.mono_run_main("Client.dll", App.runArgs.applicationArguments);
}
