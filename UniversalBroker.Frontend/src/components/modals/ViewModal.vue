<script>
import BaseModal from './BaseModal.vue';

export default{
    components:{
        BaseModal
    },
    props:{
        entityName:{
            type: String, 
            required: true, 
            default: "Сообщение"
        },
        viewedEntity:{
            type: Object, 
            required: true, 
            default: {
                id:"1",
                name:"123",
                text:"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                connections:[
                    {
                        key:"key",
                        value:"value",
                        color:"table-success"
                    },
                    {
                        key:"key",
                        value:"value",
                        color:"table-danger"
                    },
                ]
            }
        },
        struncture:{
            type: Object, 
            required: true, 
            default: [
                {
                    name: null,
                    propertyes:[
                        {
                            fieldName:"id",
                            type:"text",
                            displayName:"Id",
                            description:"описание"
                        },
                        {
                            fieldName:"text",
                            type:"text",
                            displayName:"Текст",
                            description:"описание"
                        },
                    ]
                },
                {
                    name:"Раздел 1",
                    propertyes:[
                        {
                            fieldName:"name",
                            type:"text",
                            displayName:"Имя",
                            description:"описание"
                        },
                        {
                            fieldName:"connections",
                            type:"table",
                            displayName:"Таблица",
                            description:"описание",
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
                },
            ]
        }
    },
    methods:{
        Open(){
            this.$refs.baseModal.OpenModal()
        },
        Close(){
            this.$refs.baseModal.CloseModal()
        }
    }
}
</script>

<template>
    <BaseModal 
        modalSize="lg"
        ref="baseModal"
    >
        <template v-slot:header>
            <h5 class="modal-title" id="modalTitleId">
                {{ this.entityName }}
            </h5>
        </template>
        <template v-slot:body>
            <div
                v-for="block in this.struncture"
            >
                <div 
                    class="d-flex w-100" 
                    v-if="block.name !== null"
                >
                    <hr class="hr me-2" style="width: 5%;"/>
                    <span class="text-nowrap">{{ block.name }}</span>
                    <hr class="hr w-100 ms-2" />
                </div>

                <div 
                    class="mb-3 d-flex flex-column text-left text-start" 
                    v-for="item in block.propertyes" 
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
                        class="table-responsive"
                        v-if="item.type == 'table'"
                    >
                        <table
                            class="table w-100%"
                        >
                            <thead>
                                <tr>
                                    <th 
                                        scope="col"
                                        v-for="column in item.structure"
                                    >
                                        {{column.title}}
                                    </th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr 
                                    :class="row.color" 
                                    v-for="row in this.viewedEntity[item.fieldName]"
                                >
                                    <td 
                                        scope="row"
                                        v-for="column in item.structure" 
                                    >
                                        {{ row[column.field] }}
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>                
                    <p 
                     v-else   
                    >
                        {{ this.viewedEntity[item.fieldName] }}
                    </p>

                </div>
            </div>


        </template>

        <template v-slot:footer>
        </template>
    </BaseModal>
</template>