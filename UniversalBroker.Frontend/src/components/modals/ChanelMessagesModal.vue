<script>
import ChanelApiMixine from '../../mixines/ChanelApiMixine'
import PaginationComponent from './../modules/PaginationComponent.vue'
import SearchComponent from './../modules/SearchComponent.vue'
import TableComponent from './../modules/TableComponent.vue'
import BaseModal from './BaseModal.vue'
import ViewModal from './ViewModal.vue'

export default{
    data(){
        return {
            records:[],
            PageNumber:1,
            PageSize:10,
            TotalPages:8,
            filters:[
                {
                    filterName:"startDate",
                    type:"datetime-local",
                    displayName:"Начиная с ",
                    discription:"Начальная точка временного диапозона, за который получаем логи",
                    placeholder:"",
                    weight: "fit-content",
                    value: null
                },
                {
                    filterName:"endDate",
                    type:"datetime-local",
                    displayName:"Заканчивая",
                    discription:"Конечная точка временного диапозона, за который получаем логи",
                    placeholder:"",
                    value: null
                },
                {
                    filterName:"limits",
                    type:"number",
                    displayName:"Длина текста",
                    discription:"Ограничение на длину текста для отображения",
                    placeholder:"",
                    options: [],
                    weight:"20%",
                    value: []
                },
                {
                    filterName:"direction",
                    type:"multi-select",
                    displayName:"Направление",
                    discription:"Указывает откуда и куда передавалось сообщение",
                    placeholder:"",
                    options: [],
                    weight:"20%",
                    value: []
                },

            ],
            TableStructure:[
                {
                    ProperyName: "datetime",
                    DisplayName:"Дата",
                    Type:"text",
                    CharLimit:null
                },
                {
                    ProperyName: "directionText",
                    DisplayName:"Направление",
                    Type:"text",
                    CharLimit:null,
                },
                {
                    ProperyName: "hex",
                    DisplayName:"Hex",
                    Type:"text",
                    CharLimit:null,
                },
                {
                    ProperyName: "text",
                    DisplayName:"Текст",
                    Type:"text",
                    CharLimit:null
                },
            ],
            TableActions:[
            ],
            ViewerStructure:[
                {
                    name: null,
                    propertyes:[
                        {
                            fieldName:"datetime",
                            type:"text",
                            displayName:"Дата и время",
                            description:"Дата и время получения сообщения"
                        },
                        {
                            fieldName: "directionText",
                            directionTextype:"text",
                            displayName:"Направление",
                            description:"Откуда и куда сообщение передавалось",
                            CharLimit:null,
                        },
                        {
                            fieldName: "sourceName",
                            type:"a",
                            displayName:"Источник",
                            description:"Откуда пришло сообщение",
                            emit:"OpenSource",
                            CharLimit:null,
                            getHref: (item) => item.direction == 0 ? `/connections?id=${item.sourceId}` : `/chanels?id=${item.sourceId}`
                        },
                        {
                            fieldName: "targetName",
                            type:"a",
                            displayName:"Цель",
                            description:"Куда ушло сообщение",
                            emit:"OpenTarget",
                            CharLimit:null,
                            getHref: (item) => item.direction == 2 ? `/connections?id=${item.targetId}` : `/chanels?id=${item.targetId}`
                        },
                        {
                            fieldName:"hex",
                            type:"text",
                            displayName:"Сообщение в HEX виде",
                            description:"Байты сообщение в 16-ричной системе"
                        },
                        {
                            fieldName:"text",
                            type:"text",
                            displayName:"Сообщение в текстовом виде",
                            description:"Текст сообщения, полученый из байтов"
                        },
                        {
                            fieldName:"headersEntities",
                            type:"table",
                            displayName:"Заголоки",
                            description:"Заголовки передаваемые вместе с сообщением",
                            structure:[
                                {
                                    title:"Ключ",
                                    field:"key",
                                },
                                {
                                    title:"Значение",
                                    field:"value",
                                },
                            ]
                        }
                    ]
                }
            ],
            ViewedLog:{},
            NeedBeOpened:false,
            DirectionMapping:[
                "Подключение-Канал",
                "Канал-Канал",
                "Канал-Подключение"
            ],
            channelId: "0a73478d-f462-4ff5-870d-d31095b6cef3" 
        }
    },
    mixins:[
        ChanelApiMixine
    ],
    components:{
        PaginationComponent,
        SearchComponent,
        TableComponent,
        BaseModal,
        ViewModal
    },
    watch:{
        async filters(oldFilters, newFilters){
            await this.LoadData()
        },
        async PageNumber(newPageNumber,oldPageNumber){
            await this.LoadData()
        },
        async PageSize(oldPageSize, newPageSize){
            await this.LoadData()
        },
    },
    methods:{
        async LoadData(){

            this.$emit("StartLoading")

            var res = await this.GetChanelMessages(
                this.channelId,
                this.PageSize, 
                this.PageNumber -1,
                this.filters[0].value,
                this.filters[1].value,
                this.filters[3].value
            )

            if(res.code == 200){
                this.records = res.body.page.map(x=>(
                    {
                        ...x, 
                        hex: this.toHexString(x.data),
                        headersEntities: Object.entries(x.headers).map(y=>({
                            key:y[0],
                            value:y[1]
                        })),
                        directionText:this.DirectionMapping[x.direction]
                    })
                );
                
                this.TableStructure[2].CharLimit = this.filters[2].value
                this.TableStructure[3].CharLimit = this.filters[2].value
                    
                this.TotalPages = res.body.totalPages

                if(this.PageNumber > this.TotalPages)
                    this.PageNumber = this.TotalPages

                if(this.PageNumber < 1)
                    this.PageNumber = 1
            }
            console.log(this.records)
            this.$forceUpdate()

            this.$emit("StopLoading")
        },
        toHexString(byteArray) {
            return Array.from(byteArray, function(byte) {
                    return ('0' + (byte & 0xFF).toString(16)).slice(-2) + ' ';
                }).join('')
        },
        SelfOpen(){
            if(this.NeedBeOpened){
                this.$refs.baseModal.OpenModal()
                this.NeedBeOpened = false;
            }
        },
        async OpenView(item){
            this.ViewedLog = item

            this.$refs.baseModal.CloseModal()
            this.NeedBeOpened = true;

            this.$refs.viewModal.Open()
        },
        async Open(channelId){
            this.channelId = channelId
            this.$refs.baseModal.OpenModal()
            await this.LoadData()
        },
        Close(){
            this.$refs.baseModal.CloseModal()

            console.log(this.channelId)
            this.NeedBeOpened = false;
        },
    },
    mounted(){
        this.filters[3].options = this.DirectionMapping.map((currElement, index)=>({
            "value": index,
            "label": currElement
            })
        )
    }
}
</script>

<template>
    <BaseModal
        modalSize="xl"
        title="Сообщения прошедшие по Каналу"
        ref="baseModal"
        modalId="topLavelModal"
    >
        <template v-slot:body>
            <SearchComponent
                v-model:FiltersData="this.filters"
                @applyFilter="this.LoadData" 
                :UseQueries="false"
            />

            <TableComponent
                v-model:records="this.records"
                :actions="this.TableActions"
                :structure="this.TableStructure"
                :canAdd = "false"
                @RowClick="this.OpenView"
            />

            <PaginationComponent 
                :UseQueries="false"
                v-model:TotalPages="this.TotalPages"
                v-model:PageNumber="this.PageNumber"
                v-model:PageSize = "this.PageSize"
            />
        </template>
    </BaseModal>

    <ViewModal 
        entityName="Лог"
        :struncture="this.ViewerStructure"
        :withoutFooter="true"
        v-model:viewedEntity="this.ViewedLog"
        @modalClose="this.SelfOpen"
        @OpenSource="this.OpenSource"
        @OpenTarget="this.OpenTarget"
        ref="viewModal"
    />
</template>