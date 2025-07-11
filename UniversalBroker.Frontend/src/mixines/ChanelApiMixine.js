import ApiMixines from "./ApiMixines" 

const ChanelApiMixine = {
    mixins: [ 
        ApiMixines 
    ],
    methods:{
        async GetChanelsList(pageSize, pageIndex, search){
            return await this.__CreateResponce(await this.__makeRequest(
                "GET", 
                "/api/Chanel", 
                null, 
                null, 
                {
                    pageSize: pageSize == null? 10 : pageSize,
                    pageIndex: pageIndex == null? 0 : pageIndex,
                    search: search == null? "" : search
                }
            ));
        },
        async CreateChanel(createChanelDto){
            return await this.__CreateResponce(await this.__makeRequest(
                "POST", 
                "/api/Chanel", 
                createChanelDto, 
                null, 
                null
            ));
        },
        async GetChanel(id){
            return await this.__CreateResponce(await this.__makeRequest(
                "GET", 
                `/api/Chanel/${id}`, 
                null, 
                null, 
                null
            ));
        },
        async DeleteChanel(id){
            return await this.__CreateResponce(await this.__makeRequest(
                "DELETE", 
                `/api/Chanel/${id}`, 
                null, 
                null, 
                null
            ));
        },
        async UpdateChanel(id, UpdateChanelDto){
            return await this.__CreateResponce(await this.__makeRequest(
                "PUT", 
                `/api/Chanel/${id}`, 
                UpdateChanelDto, 
                null, 
                null
            ));
        },
        async UpdateChanelScript(id, script){
            return await this.__CreateResponce(await this.__makeRequest(
                "PUT", 
                `/api/Chanel/${id}/script`, 
                script, 
                null, 
                null
            ));
        },
        async GetChanelLogs(id, pageSize, pageIndex, startInterval, stopInterval, lavels){
            return await this.__CreateResponce(await this.__makeRequest(
                "GET", 
                `/api/Chanel/${id}/logs`, 
                null, 
                null, 
                {
                    pageSize: pageSize == null? 10 : pageSize,
                    pageIndex: pageIndex == null? 0 : pageIndex,
                    startInterval: startInterval,
                    stopInterval: stopInterval,
                    lavels: lavels == null || lavels.length == 0 ? null : lavels
                }
            ));
        },
        async GetChanelMessages(id, pageSize, pageIndex, startInterval, stopInterval){
            return await this.__CreateResponce(await this.__makeRequest(
                "GET", 
                `/api/Chanel/${id}/messages`, 
                null, 
                null, 
                {
                    pageSize: pageSize == null? 10 : pageSize,
                    pageIndex: pageIndex == null? 0 : pageIndex,
                    startInterval: startInterval,
                    stopInterval: stopInterval
                }
            ));
        },
        async GetChanelRelations(id){
            return await this.__CreateResponce(await this.__makeRequest(
                "GET", 
                `/api/Chanel/${id}/relations`, 
                null, 
                null, 
                null
            ));
        },
        async GetSystemRelations(){
            return await this.__CreateResponce(await this.__makeRequest(
                "GET", 
                `/api/Chanel/relations`, 
                null, 
                null, 
                null
            ));
        }
    }
}

export default ChanelApiMixine