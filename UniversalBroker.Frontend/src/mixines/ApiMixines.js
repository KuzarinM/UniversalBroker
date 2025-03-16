// https://stackoverflow.com/questions/53010064/pass-environment-variable-into-a-vue-app-at-runtime/55962511#55962511
export default {
    data(){
        return {
            __token:"123"
        }
    },
    methods:{
        async __makeRequest(method, path, data, headers, queries){
            var request = {
                method: method,
                headers: new Headers()
            }
            request.headers.append("Authorization", `Bearer_${this.__token}`)
            request.headers.append("Content-Type", "application/json")

            if(data !== null)
                request.body = JSON.stringify(data)

            if(headers !== null){
                Object.keys(headers).forEach(key => {
                    request.headers.append(key, headers[key])
                });
            }
            
            if(queries != null)
                Object.keys(queries).forEach(key => {
                    if (queries[key] === undefined || queries[key] === null) {
                    delete queries[key];
                    }
                });
            else
                queries = {}

            var url = `${window.location.origin}/proxy${path}?${new URLSearchParams(queries).toString()}`

            return await fetch(url, request)
        },
        async __CreateResponce(rawResponce){
            var res = {
                code: rawResponce.status,
                body: null,
                headers: null
            }

            try{
                if(res.code !== 204){
                    res.body = await rawResponce.json()
                }
                
                res.headers = await rawResponce.headers
            }
            catch(ex){
                
            }

            return res;
        }
    }
}

