<script>
import BaseModal from "./BaseModal.vue"
import Multiselect from '@vueform/multiselect'
import { Codemirror } from 'vue-codemirror'
import { javascript, javascriptLanguage } from '@codemirror/lang-javascript'
import { oneDark } from '@codemirror/theme-one-dark' // Тёмная тема, если вдруг захочу
import AttributesComponent from "../modules/AttributesComponent.vue"

export default{
    data(){
        return{
            extensions:[
                javascript(), 
                javascriptLanguage.data.of({
                    autocomplete: this.myCompletions
                })
            ]
        }
    },
    props:{
        entityName:{ 
            type: String, 
            required: true, 
            default: "" 
        },
        canBeCreate:{
            type: Boolean, 
            required: true, 
            default: false 
        },
        canBeEdit:{
            type: Boolean, 
            required: true, 
            default: true 
        },
        canBeDelete:{
            type: Boolean, 
            required: true, 
            default: true 
        },
        modalFields:{
            type: Object, 
            required: true, 
            default: {
                "Дополнительные сведения":[
                    {
                        propertyName:"id",
                        displayName:"Id",
                        description:"Идентифкатор сущности",
                        type:"string",
                        readoly:true,
                    },
                    {
                        propertyName:"name",
                        displayName:"Имя",
                        description:"Имя сущности",
                        type:"string",
                        readoly:false,
                    },
                    {
                        propertyName:"tags",
                        displayName:"Теги",
                        description:"Теги сущности",
                        type:"multi-select",
                        options:[
                            {label:"A1", value:1},
                            {label:"A2", value:2},
                            {label:"A3", value:3},
                        ],
                        readoly:false,
                    },
                    {
                        propertyName:"attributes",
                        displayName:"Атррибуты",
                        description:"Атттрибуты сущности",
                        type:"attributes",
                        readoly:false,
                    },
                ],
                "Раздел 2":[
                    {
                        propertyName:"status",
                        displayName:"Статус",
                        description:"Статус сущности",
                        type:"checkbox",
                        label: "Соединение работет",
                        readoly:false,
                    },
                    {
                        propertyName:"text",
                        displayName:"Текст",
                        description:"Текст сущности",
                        type:"textarea",
                        readoly:true,
                        rows:10
                    },
                    {
                        propertyName:"code",
                        displayName:"Скрипт",
                        description:"Скртип сущности",
                        type:"codeeditor",
                        readoly:false,
                    }
                ]
            } 
        },
        displayObject:{
            type: Object, 
            required: true, 
            default: {
                id:"1",
                name:"123",
                status:true,
                tags:["1","2"],
                text:"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                code:"import CodeEditor from \'simple-code-editor\';\r\nexport default {\r\n  components: {\r\n    CodeEditor\r\n  },\r\n  data() {\r\n    return {\r\n      value: \'\'\r\n    }\r\n  }\r\n}",
                attributes: {
                    key1: "value",
                    key2: "value"
                }
            } 
        }
    },
    emits:[
        "Delete",
        "Save",
        "Create"
    ],
    components:{
        BaseModal,
        Multiselect,
        Codemirror,
        AttributesComponent
    },
    watch:{
        displayObject(newDisplayObject, oldDisplayObject){
            if(newDisplayObject.id != null && newDisplayObject.id != undefined){
                var quries = JSON.parse(JSON.stringify(this.$route.query));

                quries.id = this.displayObject.id

                console.log(quries)

                this.$router.push({path: this.$route.fullPath, query: quries, params: this.$route.params });
            }
        }
    },
    methods:{
        Create(){
            console.log(this.displayObject)
            this.$emit("Create", this.displayObject)
        },
        SaveChanges(){
            console.log("Save")
            console.log(this.displayObject)
            this.$emit("Save", this.displayObject)
        },
        Delete(){
            console.log("Delete")
            this.$emit("Delete", this.displayObject)
        },
        Open(){
            this.$refs.baseModal.OpenModal()
        },
        Close(){
            this.$refs.baseModal.CloseModal()

            this.RemoveReomQuery()
        },
        RemoveReomQuery(){
            var quries = JSON.parse(JSON.stringify(this.$route.query));

            delete quries.id

            this.$router.push({path: this.$route.fullPath, query: quries, params: this.$route.params });
        },
        myCompletions(context) {
            let word = context.matchBefore(/\w*/)
            if (word.from == word.to && !context.explicit)
                return null
            return {
                from: word.from,
                // https://codemirror.net/docs/ref/#autocomplete.Completion
                options: [
                    {
                        label: "senderId", 
                        type: "variable", 
                        apply: "Context.SenderId", 
                        detail: "macro", 
                        info:"Id скрипта, где мы сейчас"
                    },
                    {
                        label: "chanels", 
                        type: "variable", 
                        apply: "Context.Chanels[]", 
                        detail: "macro", 
                        info:"Список каналов, которые были указаны"
                    },
                    {
                        label: "connections", 
                        type: "variable", 
                        apply: "Context.Connections[]", 
                        detail: "macro", 
                        info:"Список подключений, которые были указаны"
                    },
                    {
                        label: "sendToChanel", 
                        type: "method", 
                        apply: "Context.SendMessageToChanel(Id, data, headers)", 
                        detail: "macro", 
                        info:"Отправка сообщения в канал"
                    },
                    {
                        label: "sendToCommunication", 
                        type: "method", 
                        apply: "Context.SendMessageToConnection(Id, data, headers)", 
                        detail: "macro", 
                        info:"Отправка сообщения в Подключение"
                    },
                    {
                        label: "logError", 
                        type: "method", 
                        apply: "Context.LogError(message)", 
                        detail: "macro", 
                        info:"Логируем ошибки"
                    },
                    {
                        label: "logWarning", 
                        type: "method", 
                        apply: "Context.LogWarning(message)", 
                        detail: "macro", 
                        info:"Логируем предупреждения"
                    },
                    {
                        label: "logInfo", 
                        type: "method", 
                        apply: "Context.LogInfo(message)", 
                        detail: "macro", 
                        info:"Логируем информацию"
                    },
                    {
                        label: "storageContains", 
                        type: "function", 
                        apply: "Context.CheckContainsInStorage(key)", 
                        detail: "macro", 
                        info:"Проверить наличие значения в хранилище"
                    },
                    {
                        label: "storageWrite", 
                        type: "method", 
                        apply: "Context.WriteIntoStorage(key, value)", 
                        detail: "macro", 
                        info:"Записать в хранилище по ключу"
                    },
                    {
                        label: "storageRead", 
                        type: "function", 
                        apply: "Context.ReadFromStorage(string key)", 
                        detail: "macro", 
                        info:"Прочитать из хранилища по ключу"
                    },
                    {
                        label: "messageId", 
                        type: "variable", 
                        apply: "Message.InternalId", 
                        detail: "macro", 
                        info:"Уникальный идентификатор для внутреннего сообщения"
                    },
                    {
                        label: "messageIsFromConnection", 
                        type: "variable", 
                        apply: "Message.IsFromConnection", 
                        detail: "macro", 
                        info:"Указывает, является ли сообщение исходящим от соединения"
                    },
                    {
                        label: "messageSourceId", 
                        type: "variable", 
                        apply: "Message.SourceId", 
                        detail: "macro", 
                        info:"Идентификатор источника, который отправил сообщение"
                    },
                    {
                        label: "messageData", 
                        type: "variable", 
                        apply: "Message.Data", 
                        detail: "macro", 
                        info:"Данные сообщения в формате байтов"
                    },
                    {
                        label: "messageText", 
                        type: "variable", 
                        apply: "Message.Text", 
                        detail: "macro", 
                        info:"Данные сообщения, декодированные в виде строки UTF-8"
                    },
                    {
                        label: "messageHeaders", 
                        type: "variable", 
                        apply: "Message.Headers", 
                        detail: "macro", 
                        info:"Словарь, содержащий информацию заголовков, связанную с сообщением"
                    },
                ]
            }
        }
    }
}
</script>

<template>
    <BaseModal 
        ref="baseModal"
        modalSize="lg"
        @modalClose="this.RemoveReomQuery"
    >
        <template v-slot:header>
            <h5 class="modal-title" id="modalTitleId">
                {{ this.entityName }}
            </h5>
        </template>
        <template v-slot:body>
            <div
                v-for="(block, key) in this.modalFields"
            >
                <div class="d-flex w-100">
                    <hr class="hr me-2" style="width: 5%;"/>
                    <span class="text-nowrap">{{ key }}</span>
                    <hr class="hr w-100 ms-2" />
                </div>

                <div 
                    class="mb-3 d-flex flex-column text-left text-start" 
                    v-for="item in block" 
                    :style="`width: ${item.weight};`"
                >
                    <label 
                        for="" 
                        class="d-flex form-label mb-1 justify-content-start"
                    >
                        <p class="me-1 mb-0">{{item.displayName}}</p>
                        
                        <i 
                            class="bi bi-info-circle mb-0" 
                            data-bs-toggle="tooltip" 
                            data-bs-placement="top" 
                            :title="item.description" 
                        >
                        </i>
                    </label>

                    <div
                        v-if="item.type == 'checkbox'"
                    >
                        <input 
                            class="form-check-input me-2"
                            type="checkbox"
                            v-model="this.displayObject[item.propertyName]"
                            :key="this.displayObject[item.propertyName]"
                            :indeterminate="this.displayObject[item.propertyName] == null"
                            :disabled="item.readoly"
                        />
                        <label class="form-check-label" for="">{{ item.label }}</label>
                    </div>
                    <textarea 
                        class="form-control" 
                        v-model="this.displayObject[item.propertyName]"
                        :rows="item.rows"
                        :readonly="item.readoly"
                        v-else-if="item.type == 'textarea'">

                    </textarea>
                    <Codemirror
                        v-model="this.displayObject[item.propertyName]"
                        placeholder="Code goes here..."
                        :autofocus="true"
                        :indent-with-tab="true"
                        :extensions="extensions"
                        :readonly="item.readoly"
                        doc="function mx(a){}"
                        v-else-if="item.type == 'codeeditor'"
                    />
                    <AttributesComponent 
                        v-model:AttributesDict="this.displayObject[item.propertyName]"
                        v-else-if="item.type == 'attributes'"
                    />
                    <input
                        v-else-if="item.type != 'select' && item.type != 'multi-select'"
                        :type="item.type"
                        class="form-control"
                        :disabled="item.readoly"
                        v-model="this.displayObject[item.propertyName]"
                        :placeholder="item.placeholder"
                    />
                    <Multiselect
                        v-model="this.displayObject[item.propertyName]"
                        mode="tags"
                        :close-on-select="false"
                        :searchable="true"
                        :create-option="true"
                        :options="item.options"
                        :disabled="item.readoly"
                        v-else-if ="item.type == 'multi-select'"
                    />
                    <Multiselect
                        v-model="this.displayObject[item.propertyName]"
                        :close-on-select="true"
                        :searchable="true"
                        :create-option="true"
                        :options="item.options"
                        :disabled="item.readoly"
                        v-else
                    />      
                </div>
            </div>


        </template>

        <template v-slot:footer>
            <button
                type="button"
                class="btn btn-success"
                @click.prevent="this.SaveChanges()"
                v-if="this.canBeEdit"
            >
                Сохранить
            </button>

            <button
                type="button"
                class="btn btn-success"
                @click.prevent="this.Create()"
                v-if="this.canBeCreate"
            >
                Создать
            </button>

            <button
                type="button"
                class="btn btn-danger"
                @click.prevent="this.Delete()"
                 v-if="this.canBeDelete"
            >
                Удалить
            </button>
        </template>
    </BaseModal>
</template>