<script>
export default{
    data(){
        return{
            _structure:[
                // {
                //     ProperyName: "first",
                //     DisplayName:"Солбец 1",
                //     Type:"text",
                //     CharLimit:null
                // },
                {
                    ProperyName: "second",
                    DisplayName:"Солбец 2",
                    Type:"checkbox",
                    ColumnSise:1,
                    CharLimit:null
                },
                {
                    ProperyName: "third",
                    DisplayName:"Солбец 3",
                    Type:"text",
                    ColumnSise:10,
                    CharLimit:20
                },
            ],
            _actions:[
                {
                    IconCLass:"bi-collection",
                    IsInteractive:true,
                    clickEmitName:"actionCLick"
                },
                {
                    IconCLass:"bi-journal-text",
                    IsInteractive:false,
                    clickEmitName:"actionCLick"
                },
                {
                    IconCLass:"bi-trash3",
                    IsInteractive:false,
                    clickEmitName:"actionCLick"
                },
                {
                    IconCLass:"bi-pencil",
                    IsInteractive:false,
                    clickEmitName:"actionCLick"
                },
                {
                    IconCLass:"bi-save",
                    IsInteractive:false,
                    clickEmitName:"actionCLick"
                }
            ],
            _records:[
                {
                    first:"1223",
                    second:true,
                    third:"Очень очень очень очень длинный текст"
                },
                {
                    first:"2223",
                    second:false,
                    third:"Очень очень очень очень длинный текст"
                }
            ],
        }
    },
    props:{
        structure:Array,
        actions:Array,
        records:Array,
        canAdd:Boolean
    },
    emits:[
        "RowClick",
        "AddRow"
    ],
    methods:{
        StrictText(item, property){
            if(property.CharLimit == null)
                return item[property.ProperyName]

            return item[property.ProperyName].length > property.CharLimit ? 
                item[property.ProperyName].slice(0, property.CharLimit - 1) + '…' : 
                item[property.ProperyName];
        },
        RowClick(line){
            console.log(line)
            this.$emit("RowClick", line)
        },
        ActionClick(action, row){
            console.log(action)
            this.$emit(action.clickEmitName, row)
        },
        AddRow(){
            console.log("addRow")
            this.$emit("AddRow")
        }
    }
}
</script>

<template>

    <div class="d-flex w-100">
        <hr class="hr me-2" style="width: 5%;"/>
        <span>Результат</span>
        <hr class="hr w-100 ms-2" />
    </div>

    <div
        class="table-responsive "
    >
        <table
            class="table table-light table-bordered table-hover border-dark"
        >
            <thead>
                <tr>
                    <th scope="col" v-for="item in this.structure">{{item.DisplayName}}</th>
                    <th scope="col" v-if="this.actions != null && this.actions.length >0">Действия</th>
                </tr>
            </thead>
            <tbody>
                <tr 
                    class="" 
                    v-for="item in this.records"
                >
                    <td  
                        v-for="structure in this.structure"
                        @click="this.RowClick(item)"
                    >
                        <p v-if="structure.Type != 'checkbox'">
                            {{ this.StrictText(item, structure)}}
                        </p>
                        <input
                                class="form-check-input"
                                type="checkbox"
                                v-model = "item[structure.ProperyName]"
                                disabled
                                v-else
                            />
                        
                    </td>

                    <td 
                        scope="row" 
                        class="w-auto"
                        v-if="this.actions != null && this.actions.length" 
                        @click="this.RowClick(item)"
                    >
                        <button 
                            type="button" 
                            class="btn p-0 mb-0 me-3"  
                            @click.stop="this.ActionClick(action, item)"
                            v-for="action in this.actions">
                            <i :class="`bi ${action.IconCLass}`" >
                            </i>
                        </button >
                    </td>
                </tr>
                <tr v-if="this.canAdd">
                    <td  
                    :colspan="this.structure.length + (this.actions != null && this.actions.length >0 ? 1:0)"
                    @click="this.AddRow()"
                    >
                    <p class="p-0 m-0">Добавить</p>
                    
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
    

</template>