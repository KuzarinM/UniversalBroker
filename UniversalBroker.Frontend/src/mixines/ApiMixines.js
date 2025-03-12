// https://stackoverflow.com/questions/53010064/pass-environment-variable-into-a-vue-app-at-runtime/55962511#55962511
const ApiMixines = {
    data(){
        return {
            _baseUrl:"",
            _token:""
        }
    },
    methods:{
        async MakeRequest(method, path, data, headers){

        },
        async _getTokenHeader(){

        }
    }
}

export default ApiMixines