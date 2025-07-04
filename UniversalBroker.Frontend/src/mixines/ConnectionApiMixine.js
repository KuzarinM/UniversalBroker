import ApiMixines from "./ApiMixines" 

const ConnectionApiMixine = {
    mixins: [ 
        ApiMixines 
    ],
    methods:{
        async GetConnectionsList(pageSize, pageIndex, communicationId, inputOnly, iearch){
            return await this.__CreateResponce(await this.__makeRequest(
                "GET", 
                "/api/Connection", 
                null, 
                null, 
                {
                    "pageSize": pageSize == null? 10 : pageSize,
                    "pageIndex": pageIndex? 0 : pageIndex,
                    "communicationId": communicationId,
                    "inputOnly": inputOnly,
                    "iearch": iearch == null? "" : iearch
                }
            ));
        },
        async CreateConnection(connectionDto){
            return await this.__CreateResponce(await this.__makeRequest(
                "POST", 
                "/api/Connection", 
                connectionDto, 
                null, 
                null
            ));
        },
        async GetConnection(id){
            return await this.__CreateResponce(await this.__makeRequest(
                "GET", 
                `/api/Connection/${id}`, 
                null, 
                null, 
                null
            ));
        },
        async DeleteConnection(id){
            return await this.__CreateResponce(await this.__makeRequest(
                "DELETE", 
                `/api/Connection/${id}`, 
                null, 
                null, 
                null
            ));
        },
        async UpdateConnection(id, connectionDto){
            return await this.__CreateResponce(await this.__makeRequest(
                "PUT", 
                `/api/Connection/${id}`, 
                connectionDto, 
                null, 
                null
            ));
        },
        async GetMessages(id, pageSize, pageIndex, startInterval, stopInterval){
            return await this.__CreateResponce(await this.__makeRequest(
                "GET", 
                `/api/Connection/${id}/messages`, 
                null, 
                null, 
                {
                    pageSize: pageSize == null? 10 : pageSize,
                    pageIndex: pageIndex? 0 : pageIndex,
                    startInterval: startInterval,
                    stopInterval: stopInterval
                }
            ));
        }
    }
}

export default ConnectionApiMixine