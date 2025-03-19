<script>
import CommunicationApiMixine from '../mixines/CommunicationApiMixine';
import SearchComponent from '../components/modules/SearchComponent.vue';
import PaginationComponent from '../components/modules/PaginationComponent.vue';
import TableComponent from '../components/modules/TableComponent.vue';
import EntityModal from '../components/modals/EntityModal.vue';

export default{
    data(){
        return{
            data:[],
            PageNumber:1,
            PageSize:10,
            TotalPages:8,
            filters:[
                {
                    filterName:"status",
                    type:"checkbox",
                    displayName:"Статус",
                    discription:"Работает Соединение или нет на данный момент",
                    placeholder:"",
                    weight: "fit-content",
                    value: null
                },
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
                    clickEmitName:"DeleteRow"
                }
            ],
            modalFields:{
                "Основные данные":[
                    {
                        propertyName:"id",
                        displayName:"Id соединения",
                        description:"Идентифкатор сущности",
                        type:"string",
                        readoly:true,
                    },
                    {
                        propertyName:"status",
                        displayName:"Статус",
                        description:"Статус работоспособности на данный момент",
                        type:"checkbox",
                        label: "Соединение работет",
                        readoly:true,
                    },
                    {
                        propertyName:"name",
                        displayName:"Название Соединения",
                        description:"Имя даного соединия",
                        type:"string",
                        readoly:true,
                    }   
                ],
                "Дополнительные сведения":[
                    {
                        propertyName:"attributes",
                        displayName:"Атрибуты соединения",
                        description:"Набор пар ключ-значения, через который можно настроить и сконфигурировать Соединие",
                        type:"attributes",
                        readoly:false,
                    }
                ]
            },
            editedCommunication:{},
            savedConnunication:{}
        }
    },
    mixins:[
        CommunicationApiMixine
    ],
    components:{
        SearchComponent,
        PaginationComponent,
        TableComponent,
        EntityModal
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

            this.$emit("StartLoading")

            var res = await this.GetCommunicationList(
                this.PageSize, 
                this.PageNumber -1 , 
                this.filters[0].value, 
                this.filters[1].value)

            if(res.code == 200){
                this.data = res.body.page;

                this.TotalPages = res.body.totalPages

                if(this.PageNumber > this.TotalPages)
                    this.PageNumber = this.TotalPages

                if(this.PageNumber < 1)
                    this.PageNumber = 1
            }

            this.$emit("StopLoading")
        },
        async OpenCommunication(item){
            this.$emit("StartLoading")

            var res = await this.GetCommunication(item.id)

            if(res.code == 200){
                this.editedCommunication = res.body

                // Сохраняем модель старую
                this.savedConnunication = JSON.parse(JSON.stringify(res.body))

                this.$refs.updateModal.Open()
            }
            else{
                alert('Не удалось загрузить данные о соединении')
            }

            this.$emit("StopLoading")
        },
        async DeleteCommunication(item){
            if(!confirm(`Вы уверены, что хотите удалить подключение ${item.name}`))
                return

            alert("Для теста функцию заблокировал")
            return

            this.$emit("StartLoading")
            var res = await this.DeleteCommunication(item.id)

            if(res.code == 200){
                console.log("Удаляем", item)
                this.$refs.updateModal.Close()
            }
            else{
                alert("Не удалось удалить Соединение")
            }

            await this.LoadData();

            this.$emit("StopLoading")
        },
        async SaveCommunication(item){
            var attributes = JSON.parse(JSON.stringify(item.attributes))

            for (let key in this.savedConnunication.attributes) {
                if(attributes[key] == undefined){
                    attributes[key] = null
                }

                if(attributes[key] == this.savedConnunication.attributes[key]){
                    delete attributes[key]
                }
            }

            var res = await this.UpdateCommunication(item.id, attributes)

            if(res.code == 200){
                this.editedCommunication = res.body
                this.savedConnunication = JSON.parse(JSON.stringify(res.body))
            }
            else{
                alert("Не удалось сохранить изменения Соединения")
            }

            await this.LoadData();
        }
    },
     async mounted(){

        await this.$router.isReady()

        var id = this.$route.query.id

        if(id != null && id != undefined){
            await this.OpenCommunication({
                id:id
            })
        }
        //await this.LoadData();
    }
}
</script>

<template>
    <h2 class="text-start">Cписок Подключений</h2>

    <SearchComponent
        v-model:FiltersData="this.filters"
        @applyFilter="this.LoadData" 
    />

    <TableComponent
        v-model:records="this.data"
        :actions="this.TableActions"
        :structure="this.TableStructure"
        :canAdd = "false"
        @RowClick="this.OpenCommunication"
        @DeleteRow="this.DeleteCommunication"
    />

    <EntityModal
        v-model:displayObject="this.editedCommunication"
        :modalFields="this.modalFields"
        entityName="Просмотр Соединения"
        :canBeCreate="false"
        :canBeEdit="true"
        :canBeDelete="true"
        @Delete = "this.DeleteCommunication"
        @Save="this.SaveCommunication"
        ref="updateModal"
     />

    <PaginationComponent 
        v-model:TotalPages="this.TotalPages"
        v-model:PageNumber="this.PageNumber"
        v-model:PageSize = "this.PageSize"
    />
</template>