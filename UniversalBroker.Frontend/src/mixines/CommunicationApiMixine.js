import ApiMixines from "./ApiMixines" 

const CommunicationApiMixine = {
    mixins: [ 
        ApiMixines 
    ],
    methods:{
        async GetCommunicationList(pageSize, pageIndex, status, search){
            return await this.__CreateResponce(await this.__makeRequest(
                "GET", 
                "/api/Communication", 
                null, 
                null, 
                {
                    pageSize: pageSize == null ? 10 : pageSize,
                    pageIndex: pageIndex == null || pageIndex < 0 ? 0 : pageIndex,
                    Status: status,
                    Search: search
                }
            ));
        },
        async CreateCommunication(createCommunicationDto){
            return await this.__CreateResponce(await this.__makeRequest(
                "POST", 
                "/api/Communication",
                createCommunicationDto, 
                null, 
                null
            ))
        },  
        async GetCommunication(id){
            return await this.__CreateResponce(await this.__makeRequest(
                "GET", 
                `/api/Communication/${id}`,
                null, 
                null, 
                null
            ))
        },
        async DeleteCommunication(id){
            return await this.__CreateResponce(await this.__makeRequest(
                "DELETE", 
                `/api/Communication/${id}`,
                null, 
                null, 
                null
            ))
        },
        async DeleteCommunication(id, headers){
            return await this.__CreateResponce(await this.__makeRequest(
                "PATCH", 
                `/api/Communication/${id}`,
                headers, 
                null, 
                null
            ))
        }
    }
}

export default CommunicationApiMixine