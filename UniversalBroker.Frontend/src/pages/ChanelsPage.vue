<script>
import ChanelApiMixine from '../mixines/ChanelApiMixine';
import ConnectionApiMixine from '../mixines/ConnectionApiMixine';
import PaginationComponent from '../components/modules/PaginationComponent.vue';
import SearchComponent from '../components/modules/SearchComponent.vue';
import TableComponent from '../components/modules/TableComponent.vue';
import EntityModal from '../components/modals/EntityModal.vue';
import ViewModal from '../components/modals/ViewModal.vue'
import ChanelLogsModal from '../components/modals/ChanelLogsModal.vue';

export default{
    data(){
        return{
            data:[],
            connections:[],
            channels:[],
            PageNumber:1,
            PageSize:10,
            TotalPages:8,
            filters:[
                {
                    filterName:"search",
                    type:"text",
                    displayName:"Поиск",
                    discription:"Поиск по содержимому названия",
                    placeholder:"Введите поиск",
                    weight: "30%",
                    value: null
                },
            ],
            TableStructure:[
                {
                    ProperyName: "name",
                    DisplayName:"Название",
                    Type:"text",
                    CharLimit:null
                },
            ],
            TableActions:[
                {
                    IconCLass:"bi-collection",
                    clickEmitName:"OpenMessages"
                },
                {
                    IconCLass:"bi-journal-text",
                    clickEmitName:"OpenLogs"
                },
                {
                    IconCLass:"bi-trash3",
                    clickEmitName:"DeleteRow"
                },
            ],
            editorFields:{
                "Основные данные":[
                    {
                        propertyName:"id",
                        displayName:"Id Канала",
                        description:"Идентифкатор сущности",
                        type:"string",
                        readoly:true,
                    },
                    {
                        propertyName:"name",
                        displayName:"Название Канала",
                        description:"Имя даного Канала",
                        type:"string",
                        readoly:false,
                    }   
                ],
                "Связи":[
                    {
                        propertyName:"inputConnections",
                        displayName:"Входные Подключения",
                        description:"Список привязанных входных подключений",
                        type:"multi-select",
                        options:[],
                        readoly:false,
                    },
                    {
                        propertyName:"outputConnections",
                        displayName:"Выходные Подключения",
                        description:"Список привязанных выходных подключений",
                        type:"multi-select",
                        options:[],
                        readoly:false,
                    },
                    {
                        propertyName:"outputChanels",
                        displayName:"Выходные Каналы",
                        description:"Список привязанных выходных Каналов",
                        type:"multi-select",
                        options:[],
                        readoly:false,
                    },
                ],
                "Скрипт":[
                    {
                        propertyName:"script",
                        displayName:"Скрипт",
                        description:"Основной скрипт-обработчки внутри данного Канала",
                        type:"codeeditor",
                        readoly:false,
                    }
                ]
            },
            creatorFields:{
                "Основные данные":[
                    {
                        propertyName:"name",
                        displayName:"Название Канала",
                        description:"Имя даного Канала",
                        type:"string",
                        readoly:false,
                    }   
                ],
                "Связи":[
                    {
                        propertyName:"inputConnections",
                        displayName:"Входные Подключения",
                        description:"Список привязанных входных подключений",
                        type:"multi-select",
                        options:[],
                        readoly:false,
                    },
                    {
                        propertyName:"outputConnections",
                        displayName:"Выходные Подключения",
                        description:"Список привязанных выходных подключений",
                        type:"multi-select",
                        options:[],
                        readoly:false,
                    },
                    {
                        propertyName:"outputChanels",
                        displayName:"Выходные Каналы",
                        description:"Список привязанных выходных Каналов",
                        type:"multi-select",
                        options:[],
                        readoly:false,
                    },
                ],
                "Скрипт":[
                    {
                        propertyName:"script",
                        displayName:"Скрипт",
                        description:"Основной скрипт-обработчки внутри данного Канала",
                        type:"codeeditor",
                        readoly:false,
                    }
                ]
            },
            editedChanel:{},
            createdChanel:{
                name:"",
                inputConnections:[],
                outputConnections:[],
                outputChanels:[],
                script:""
            },
            viewedChannelId:null
        }
    },
    mixins:[
        ChanelApiMixine,
        ConnectionApiMixine
    ],
    components:{
        PaginationComponent,
        SearchComponent,
        TableComponent,
        EntityModal,
        ViewModal,
        ChanelLogsModal
    },
    watch:{
        async filters(oldFilters, newFilters){
            await this.LoadData()
        },
        async PageNumber(oldPageNumber, newPageNumber){
            await this.LoadData()
        },
        async PageSize(oldPageSize, newPageSize){
            await this.LoadData()
        },
    },
    methods:{
        async LoadData(){

            var connectionTask = this.LoadConnections()
            var channelsTask = this.LoadChannels()

            var res = await this.GetChanelsList(
                this.PageSize, 
                this.PageNumber - 1, 
                this.filters[0].value)

            if(res.code == 200){
                this.data = res.body.page;

                this.TotalPages = res.body.totalPages

                if(this.PageNumber > this.TotalPages)
                    this.PageNumber = this.TotalPages

                if(this.PageNumber < 1)
                    this.PageNumber = 1
            }

            await connectionTask
            await channelsTask
        },
        async LoadConnections(){
            var connections = await this.GetConnectionsList(100,0)

        if(connections.code == 200){
            this.connections = connections.body.page.map(x=> ({
                "value": x.id,
                "label": x.name,
                "isInput":x.isInput
            }))

            console.log(this.connections)

            this.editorFields["Связи"][0].options = this.connections.filter(x=>x.isInput)
            this.editorFields["Связи"][1].options = this.connections.filter(x=>!x.isInput)

            this.creatorFields["Связи"][0].options = this.connections.filter(x=>x.isInput)
            this.creatorFields["Связи"][1].options = this.connections.filter(x=>!x.isInput)
        }
        },
        async LoadChannels(){
            var channels = await this.GetChanelsList(100,0)

            if(channels.code == 200){
                this.channels = channels.body.page.map(x=> ({
                    "value": x.id,
                    "label": x.name
                }))

                this.editorFields["Связи"][2].options = this.channels

                this.creatorFields["Связи"][2].options = this.channels
                //this.createModalFields["Связи"][0].options = this.communications
            }
        },
        async OpenEdit(item){
            var res = await this.GetChanel(item.id)

            if(res.code == 200){
                var raw = res.body

                raw.outputChanels = raw.outputChanels.map(x=>x.id)	
                raw.inputConnections = raw.inputConnections.map(x=>x.id)	
                raw.outputConnections = raw.outputConnections.map(x=>x.id)	

                console.log(raw)

                this.editedChanel = raw

                this.$refs.updateModal.Open()
            }
            else{
                alert('Не удалось загрузить данные о соединении')
            }
        },
        OpenAdd(){
            this.$refs.createModal.Open()
        },
        async Add(item){
            if(item.name === ""){
                alert("Имя канала явялется обязательным")
                return
            }

            var res = await this.CreateChanel(item)

            if(res.code == 200){
                this.createdChanel = {
                    name:"",
                    inputConnections:[],
                    outputConnections:[],
                    outputChanels:[],
                    script:""
                }

                this.$refs.createModal.Close()

                await this.LoadData();
            }
            else{
                alert("Не удалось создать Канал")
            }
        },
        async Delete(item){
            if(!confirm(`Вы уверены, что хотите удалить Канал ${item.name}`))
                return
            
            var res = await this.DeleteChanel(item.id)

            if(res.code == 200){
                this.$refs.updateModal.Close()
            }
            else{
                alert("Не удалось удалить Канал")
            }
            await this.LoadData();
        },
        async Update(item){
            var res = await this.UpdateChanel(item.id, item)

            if(res.code == 200){
                this.editedConnection = res.body
            }
            else{
                alert("Не удалось обновить Канал")
            }

            await this.LoadData();
        },
        async OpenLogs(item){
            await this.$refs.logsModal.Open(item.id)
        }
    }
}
</script>

<template>
<h2 class="text-start">Cписок Каналов</h2>

<SearchComponent
    v-model:FiltersData="this.filters"
    @applyFilter="this.LoadData" 
/>

<TableComponent
    v-model:records="this.data"
    :actions="this.TableActions"
    :structure="this.TableStructure"
    :canAdd = "true"
    @RowClick="this.OpenEdit"
    @AddRow="this.OpenAdd"
    @DeleteRow="this.Delete"
    @OpenLogs = "this.OpenLogs"
/>

<EntityModal
    v-model:displayObject="this.editedChanel"
    :modalFields="this.editorFields"
    entityName="Просмотр Канала"
    :canBeCreate="false"
    :canBeEdit="true"
    :canBeDelete="true"
    @Delete = "this.Delete"
    @Save="this.Update"
    ref="updateModal"
 />

 <EntityModal
    v-model:displayObject="this.createdChanel"
    :modalFields="this.creatorFields"
    entityName="Создание Канала"
    :canBeCreate="true"
    :canBeEdit="false"
    :canBeDelete="false"
    @Create="this.Add"
    ref="createModal"
 />

<ChanelLogsModal 
    ref="logsModal"
/>

<PaginationComponent 
    v-model:TotalPages="this.TotalPages"
    v-model:PageNumber="this.PageNumber"
    v-model:PageSize = "this.PageSize"
/>
</template>