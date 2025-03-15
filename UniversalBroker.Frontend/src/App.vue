<script>
import SearchComponent from "./components/modules/SearchComponent.vue"
import PaginationComponent from "./components/modules/PaginationComponent.vue"
import CommunicationApiMixine from './mixines/CommunicationApiMixine'
import TableComponent from "./components/modules/TableComponent.vue"
import ViewModal from "./components/modals/ViewModal.vue"
import AttributesComponent from "./components/modules/AttributesComponent.vue"

export default{
    data(){
        return{
            data:[],
            PageNumber:1,
            PageSize:1,
            TotalPages:8,
            filters:[
                {
                    filterName:"status",
                    type:"checkbox",
                    displayName:"Статус",
                    discription:"Описание статуса",
                    placeholder:"",
                    weight: "fit-content",
                    value: null
                },
                {
                    filterName:"search",
                    type:"text",
                    displayName:"Поиск",
                    discription:"Описание фильра",
                    placeholder:"Введите поиск",
                    weight: "30%",
                    value: null
                },
            ],
            TableStructure:[
                {
                    ProperyName: "status",
                    DisplayName:"Статус",
                    Type:"checkbox",
                    CharLimit:null
                },
                {
                    ProperyName: "name",
                    DisplayName:"Название",
                    Type:"text",
                    CharLimit:null
                },
            ],
            TableActions:[
                {
                    IconCLass:"bi-trash3",
                    clickEmitName:"deleteRow"
                }
            ]
        }
    },
    mixins:[
        CommunicationApiMixine
    ],
    components:{
        SearchComponent,
        PaginationComponent,
        TableComponent,
        ViewModal,
        AttributesComponent
    },
    watch:{
        async filters(oldFilters, newFilters){
            console.log(1)
            await this.LoadData()
        },
        async PageNumber(oldPageNumber, newPageNumber){
            console.log(2)
            await this.LoadData()
        },
        async PageSize(oldPageSize, newPageSize){
            console.log(3)
            await this.LoadData()
        },
    },
    methods:{
        async LoadData(){
            var res = await this.GetCommunicationList(
                this.PageSize, 
                this.PageNumber -1 , 
                this.filters[0].value, 
                this.filters[1].value)

            this.data = res.body.page;

            this.TotalPages = res.body.totalPages

            if(this.PageNumber > this.TotalPages)
                this.PageNumber = this.TotalPages
            
            if(this.PageNumber < 1)
                this.PageNumber = 1
        }
    },
    async mounted(){

        await this.LoadData();

        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
        var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    })

    }
}
</script>

<template>
    <SearchComponent
     v-model:FiltersData="this.filters"
     @applyFilter="this.LoadData" />

     <TableComponent
        v-model:records="this.data"
        :actions="this.TableActions"
        :structure="this.TableStructure"
        :canAdd = "true"
         />

    <ViewModal />

    <PaginationComponent 
        v-model:TotalPages="this.TotalPages"
        v-model:PageNumber="this.PageNumber"
        v-model:PageSize = "this.PageSize"
    />
</template>

<style scoped>
</style>
