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
            
            var out = [];

            if(queries != null)
                Object.keys(queries).forEach(key => {
                    if (queries[key] === undefined || queries[key] === null || queries[key] === '') {
                        delete queries[key];
                    }
                    else if(Array.isArray(queries[key])){
                        for (let index = 0; index < queries[key].length; index++) {
                            const element = queries[key][index];
                            out.push(`${key}=${element}`)
                        }
                    }
                    else{
                        out.push(`${key}=${queries[key]}`)
                    }
                }); 
                
            console.log(out)

            var url = `${window.location.origin}/proxy${path}?${out.join('&')}`

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

