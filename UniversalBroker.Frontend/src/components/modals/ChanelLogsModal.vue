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
                    filterName:"lavels",
                    type:"multi-select",
                    displayName:"Уровень",
                    discription:"Уровни логирования, которые будут запрашиваться",
                    placeholder:"",
                    options: [],
                    weight:"20%",
                    value: []
                },

            ],
            TableStructure:[
                {
                    ProperyName: "dateTime",
                    DisplayName:"Дата",
                    Type:"text",
                    CharLimit:null
                },
                {
                    ProperyName: "lavelText",
                    DisplayName:"Уровень",
                    Type:"text",
                    CharLimit:null
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
                            fieldName:"dateTime",
                            type:"text",
                            displayName:"Дата и время",
                            description:"Время лога"
                        },
                        {
                            fieldName:"lavelText",
                            type:"text",
                            displayName:"Уровень",
                            description:"Уровень ошибки"
                        },
                        {
                            fieldName:"text",
                            type:"text",
                            displayName:"Текст лога",
                            description:"Непосредственно сам тест"
                        }
                    ]
                }
            ],
            ViewedLog:{},
            NeedBeOpened:false,
            LogLavelMapping:[
                "Trace",
                "Debug",
                "Information",
                "Warning",
                "Error",
                "Critical"
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

            console.log(this.filters);

            var res = await this.GetChanelLogs(
                this.channelId,
                this.PageSize, 
                this.PageNumber -1, 
                this.filters[0].value, 
                this.filters[1].value,
                JSON.parse(JSON.stringify(this.filters[2].value))
            )

            if(res.code == 200){
                this.records = res.body.page.map(x=>(
                    {
                        ...x, 
                        lavelText:this.LogLavelMapping[x.lavel]
                    }));
                

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
        }
    },
    mounted(){
        this.filters[2].options = this.LogLavelMapping.map((currElement, index)=>({
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
        title="Логи скрипта канала"
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
        ref="viewModal"
    />
</template>