<script>
import ConnectionApiMixine from '../mixines/ConnectionApiMixine'
import CommunicationApiMixine from '../mixines/CommunicationApiMixine';
import ChanelApiMixine from '../mixines/ChanelApiMixine'
import SearchComponent from '../components/modules/SearchComponent.vue';
import PaginationComponent from '../components/modules/PaginationComponent.vue';
import TableComponent from '../components/modules/TableComponent.vue';
import EntityModal from '../components/modals/EntityModal.vue';

export default{
    data(){
        return{
            data:[],
            communications:[],
            channels:[],
            PageNumber:1,
            PageSize:10,
            TotalPages:8,
            filters:[
                {
                    filterName:"isInput",
                    type:"checkbox",
                    displayName:"Тип",
                    discription:"Является ли данное Подключение входящим(true) или исходящим(false)",
                    placeholder:"",
                    weight: "fit-content",
                    value: null
                },
                {
                    filterName:"communicationId",
                    type:"select",
                    displayName:"Выбор соединения",
                    discription:"Соединение, к оторому относится Подключение",
                    placeholder:"Все",
                    options:this.communications,
                    weight: "30%",
                    value: null
                },
                {
                    filterName:"search",
                    type:"text",
                    displayName:"Поиск",
                    discription:"Поиск по имени Подключения",
                    placeholder:"Введите поиск",
                    weight: "30%",
                    value: null
                },
            ],
            TableStructure:[
                {
                    ProperyName: "isInput",
                    DisplayName:"In/Out",
                    Type:"checkbox",
                    CharLimit:null
                },
                {
                    ProperyName: "name",
                    DisplayName:"Название соединения",
                    Type:"text",
                    CharLimit:null
                },
                {
                    ProperyName: "path",
                    DisplayName:"Путь",
                    Type:"text",
                    CharLimit:null
                },
            ],
            TableActions:[
                {
                    IconCLass:"bi-trash3",
                    clickEmitName:"DeleteRow"
                }
            ],
            editModalFields:{
                "Основные данные":[
                    {
                        propertyName:"id",
                        displayName:"Id подключения",
                        description:"Идентифкатор Подключения",
                        type:"string",
                        readoly:true,
                    },
                    {
                        propertyName:"isInput",
                        displayName:"Тип подключения",
                        description:"Является ли данное Подключение входящим(true) или исходящим(false)",
                        type:"checkbox",
                        readoly:true,
                    },
                    {
                        propertyName:"path",
                        displayName:"Путь подключения",
                        description:"Основной способ идентифицировать Подключение в адаптере. Это имя топика в Kafka или Ip адрес в случае TCP",
                        type:"string",
                        readoly:true,
                    }, 
                    {
                        propertyName:"name",
                        displayName:"Название подключения",
                        description:"Имя подключения, по которому к нему можно обратиться",
                        type:"string",
                        readoly:false,
                    },
                ],
                "Связи":[
                    {
                        propertyName:"communicationId",
                        displayName:"Соединение",
                        description:"Соединение, к которому относится Подключение",
                        type:"select",
                        options:this.communications,
                        readoly:true
                    },
                    {
                        propertyName:"channelsIds",
                        displayName:"Каналы",
                        description:"Списко каналов, связанных с Подключением (куда отправляем для входного или откуда может прилететь для выходного)",
                        type:"multi-select",
                        options:[],
                        readoly:false
                    }
                ],
                "Дополнительные сведения":[
                    {
                        propertyName:"attribues",
                        displayName:"Атрибуты Подключения",
                        description:"Атттрибуты, служащие для настройки Подключения",
                        type:"attributes",
                        readoly:false,
                    }
                ]
            },
            createModalFields:{
                "Основные данные":[
                    {
                        propertyName:"isInput",
                        displayName:"Тип Подключение",
                        description:"Входные подключение (true) используются для ввода данных, выходные(false) - для вывода",
                        type:"checkbox",
                        readoly:false,
                    },
                    {
                        propertyName:"name",
                        displayName:"Название Подключения",
                        description:"Имя, по которому будут обращаться к Подключению",
                        type:"string",
                        readoly:false,
                    },
                    {
                        propertyName:"path",
                        displayName:"Путь Подключения",
                        description:"Строка определяющая подключение. Для каждого типа Соединения это что-то своё: url, топик, Ip",
                        type:"string",
                        readoly:false,
                    },   
                ],
                "Связи":[
                    {
                        propertyName:"communicationId",
                        displayName:"Соединение",
                        description:"Соединение, к которому относится Подключение",
                        type:"select",
                        options:this.communications,
                        readoly:false
                    },
                    {
                        propertyName:"channelsIds",
                        displayName:"Подключенные каналы",
                        description:"Список каналов, связанных с Подключением. В случае входного - те куда пишем, в случае выходного - те откуда могут написать",
                        type:"multi-select",
                        options:[],
                        readoly:false
                    }
                ],
                "Дополнительные сведения":[
                    {
                        propertyName:"attribues",
                        displayName:"Атрибуты Подключения",
                        description:"Словарь ключ-значение, позволяющий настраивать Подключение. Подробности зависят от Адаптера",
                        type:"attributes",
                        readoly:false,
                    }
                ]
            },
            editedConnection:{},
            createdConnection:{
                name:"",
                path:"",
                attribues:{},
                communicationId:"",
                isInput:null,
                channelsIds:[]
            }
        }
    },
    mixins:[
        ConnectionApiMixine,
        CommunicationApiMixine,
        ChanelApiMixine
    ],
    components:{
        SearchComponent,
        PaginationComponent,
        TableComponent,
        EntityModal
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
            this.$emit("StartLoading")

            var connectionTask = this.LoadCommunications()
            var channelTask = this.LoadChannels()

            var res = await this.GetConnectionsList(
                this.PageSize, 
                this.PageNumber -1,
                this.filters[1].value,
                this.filters[0].value,
                this.filters[2].value
            )

            if(res.code == 200){
                this.data = res.body.page;

                this.TotalPages = res.body.totalPages

                if(this.PageNumber > this.TotalPages)
                    this.PageNumber = this.TotalPages

                if(this.PageNumber < 1)
                    this.PageNumber = 1
            }

            await connectionTask
            await channelTask

            this.$emit("StopLoading")
        },
        async LoadChannels(){
            var channels = await this.GetChanelsList(100,0)

            if(channels.code == 200){
                this.channels = channels.body.page.map(x=>({
                "value": x.id,
                "label": x.name
                }))

                this.editModalFields["Связи"][1].options = this.channels
                this.createModalFields["Связи"][1].options = this.channels
            }
        },
        async LoadCommunications(){
            var communications = await this.GetCommunicationList(100,0)

            if(communications.code == 200){
                this.communications = communications.body.page.map(x=> ({
                    "value": x.id,
                    "label": x.name
                }))

                this.filters[1].options = this.communications
                this.editModalFields["Связи"][0].options = this.communications
                this.createModalFields["Связи"][0].options = this.communications
            }
        },
        async Open(item){
            this.$emit("StartLoading")

            var res = await this.GetConnection(item.id)

            if(res.code == 200){
                this.editedConnection = res.body
                this.$refs.updateModal.Open()
            }
            else{
                alert("Не удлось открыть Подключение")
            }

            this.$emit("StopLoading")
        },
        async OpenAddConnection(){
            this.$refs.createModal.Open()
        },
        async Add(item){
            if(item.name === ""){
                alert("Имя должно быть заполнено")
                return;
            }

            if(item.path === ""){
                alert("Путь должен быть заполнен")
                return;
            }

            if(item.communicationId === ""){
                alert("Соединение должны быть выбрано")
                return;
            }

            if(item.isInput === null){
                alert("Тип Подключения должен быть выбран")
                return;
            }

            this.$emit("StartLoading")

            var res = await this.CreateConnection(item)

            if(res.code == 200){
                this.createdConnection = {
                    name:"",
                    path:"",
                    attribues:{},
                    communicationId:"",
                    isInput:null,
                    channelsIds:[]
                }

                this.$refs.createModal.Close()

                await this.LoadData();
            }
            else{
                alert("Не удалось создать Подключение")
            }

            this.$emit("StopLoading")
        },
        async Delete(item){
            if(!confirm(`Вы уверены, что хотите удалить Подключение ${item.name}`))
                return
            
            this.$emit("StartLoading")
            
            var res = await this.DeleteConnection(item.id)

            if(res.code == 200){
                this.$refs.updateModal.Close()
            }
            else{
                alert("Не удалось удалить Подключение")
            }
            await this.LoadData();
        },
        async Update(item){
            var res = await this.UpdateConnection(item.id, item)

            if(res.code == 200){
                this.editedConnection = res.body
            }
            else{
                alert("Не удалось обновить Подключение")
            }

            await this.LoadData();
        }
    },
     async mounted(){
        //await this.LoadData(); // Фильтратор вызовет

        await this.$router.isReady()

        var id = this.$route.query.id

        if(id != null && id != undefined){
            await this.Open({
                id:id
            })
        }
    }
}
</script>

<template>
    <h2 class="text-start">Cписок Соединений</h2>

    <SearchComponent
        v-model:FiltersData="this.filters"
        @applyFilter="this.LoadData" 
    />

    <TableComponent
        v-model:records="this.data"
        :actions="this.TableActions"
        :structure="this.TableStructure"
        :canAdd = "true"
        @RowClick="this.Open"
        @DeleteRow="this.Delete"
        @AddRow="this.OpenAddConnection"
    />

    <EntityModal
        v-model:displayObject="this.editedConnection"
        :modalFields="this.editModalFields"
        entityName="Просмотр Подключения"
        :canBeCreate="false"
        :canBeEdit="true"
        :canBeDelete="true"
        @Delete = "this.Delete"
        @Save="this.Update"
        ref="updateModal"
     />

    <EntityModal
        v-model:displayObject="this.createdConnection"
        :modalFields="this.createModalFields"
        entityName="Создание Подключения"
        :canBeCreate="true"
        :canBeEdit="false"
        :canBeDelete="false"
        @Create = "this.Add"
        ref="createModal"
    />

    <PaginationComponent 
        v-model:TotalPages="this.TotalPages"
        v-model:PageNumber="this.PageNumber"
        v-model:PageSize = "this.PageSize"
    />
</template>