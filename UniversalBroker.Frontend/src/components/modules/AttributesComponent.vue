<script>
export default{
    data(){
        return{
            attributes:[
            ]
        }
    },
    props:{
        AttributesDict:{
            type: Object, 
            required: true, 
            default: {} 
        }
    },
    emits:[ 
        "update:AttributesDict"
    ],
    methods:{
        StartEdinting(item){
            item.isEditing = true
            this.$forceUpdate();
        },
        SaveLine(item){
            item.isEditing = false
            this.$forceUpdate();

            this.UpdateProps();
        },
        AddLine(){
            this.attributes.push(
                {
                    key: "",
                    value: "",
                    isEditing: true,
                }
            )
        },
        DeleteLine(item){
            var index = this.attributes.indexOf(item);
            if (index !== -1) {
                this.attributes.splice(index, 1);
            }
            this.$forceUpdate();

            this.UpdateProps();
        },
        UpdateAttributes(){

            this.attributes = []

            for (const [key, value] of Object.entries(this.AttributesDict)) {
                this.attributes.push({
                    key: key,
                    value: value,
                    isEditing: false,
                })
            }

            console.log(this.attributes)
        },
        UpdateProps(){
            for (let index = 0; index < this.attributes.length; index++) {
                const element = this.attributes[index];
                
                if(
                    this.AttributesDict[element.key] == null || 
                    this.AttributesDict[element.key] == undefined ||
                    this.AttributesDict[element.key] !== element.value
                ){
                    this.AttributesDict[element.key] = element.value
                }
            }

            for (const [key, value] of Object.entries(this.AttributesDict)) {

                if(this.attributes.filter(x=>x.key == key).length ==0 )
                    delete this.AttributesDict[key]
            }

            this.$emit('update:AttributesDict', this.AttributesDict)
        }
    },
    mounted(){
        this.UpdateAttributes();
    }
}
</script>

<template>
<div
    class="table-responsive"
>
    <table
        class="table w-auto"
    >
        <thead>
            <tr>
                <th scope="col">Ключ</th>
                <th scope="col">Значение</th>
                <th scope="col"></th>
            </tr>
        </thead>
        <tbody>
            <tr class="" v-for="item in this.attributes">
                <td scope="row" :key="item.restart">
                    <input 
                        class="text-start form-control border-0 bg-transparent"
                        v-model="item.key"
                        readonly
                        v-if="!item.isEditing"
                        
                    />
                    <input
                        type="text"
                        class="form-control"
                        style="width: fit-content;"
                        v-model="item.key"
                        v-else
                    />
                </td>
                <td>
                    <input 
                        class="text-start form-control border-0 bg-transparent"
                        v-model="item.value"
                        readonly
                        v-if="!item.isEditing"
                        
                    />
                    <input
                        type="text"
                        class="form-control"
                        style="width: fit-content;"
                        v-model="item.value"
                        v-else
                    />
                </td>
                
                <td>
                    <button 
                        type="button" 
                        class="btn p-0 mb-0 me-3"  
                        @click="this.SaveLine(item)"
                        v-if="item.isEditing"
                    >
                        <i class="bi bi-save" >
                        </i>
                    </button >
                    <button 
                        type="button" 
                        class="btn p-0 mb-0 me-3"  
                        @click="this.StartEdinting(item)"
                        v-else
                    >
                        <i class="bi bi-pencil" >
                        </i>
                    </button >

                    <button 
                        type="button" 
                        class="btn p-0 mb-0 me-3" 
                        @click="this.DeleteLine(item)" 
                    >
                        <i class="bi bi-trash3" >
                        </i>
                    </button >
                </td>
            </tr>
            <tr class="" @click="this.AddLine()">
                <td class="text-center" colspan="3">Добавить</td>
            </tr>
        </tbody>
    </table>
</div>

</template>