<script>
import ViewModal from './ViewModal.vue';
import BaseModal from './BaseModal.vue';
import ChanelApiMixine from '../../mixines/ChanelApiMixine';
import { Network } from 'vue3-visjs'
import { color } from '@codemirror/theme-one-dark';

export default{
    data(){
        return{
            relationViewStruecture:[
                {
                    name:null,
                    propertyes:[
                        {
                            fieldName:"chanels",
                            type:"table",
                            displayName:"Реальные связи с каналами",
                            description:"Каналы, в которые отпраяли сообщения или в которые объявлена такая возможность",
                            structure:[
                                {
                                    title:"Имя объекта",
                                    field:"relationName",
                                    hrefFunc: this.GetRelationUrl
                                },
                                {
                                    title:"Статус",
                                    field:"StatusString",
                                },
                            ],
                        },
                        {
                            fieldName:"inConnections",
                            type:"table",
                            displayName:"Реальные связи с входными подключениями",
                            description:"Входные подключения, в которые отпраяли сообщения или в которые объявлена такая возможность",
                            structure:[
                                {
                                    title:"Имя объекта",
                                    field:"relationName",
                                    hrefFunc: this.GetRelationUrl
                                },
                                {
                                    title:"Статус",
                                    field:"StatusString",
                                },
                            ],
                        },
                        {
                            fieldName:"outConnections",
                            type:"table",
                            displayName:"Реальные связи с выходными подключениями",
                            description:"Выходные подключения, в которые отпраяли сообщения или в которые объявлена такая возможность",
                            structure:[
                                {
                                    title:"Имя объекта",
                                    field:"relationName",
                                    hrefFunc: this.GetRelationUrl
                                },
                                {
                                    title:"Статус",
                                    field:"StatusString",
                                },
                            ],
                        }
                    ]
                },
            ],
            viewedRelationsModel:{},
            RelationUsageStatusString:[
                "Используется", 
                "Не используется", 
                "Не отмечено"
            ],
            nodes: [],
            edges: [
                // { from: 1, to: 2, label: 'Relation' },
                // { from: 2, to: 3, arrows: 'to' },
                // { from: 3, to: 1 }
            ],
            options: {
                nodes: {
                    shape: 'box',
                    font: {
                        size: 14
                    },
                    argin: 10,
                    widthConstraint: {
                        minimum: 100,
                        maximum: 200
                    }
                },
                edges: {
                    arrows: 'to',
                    smooth: true
                },
                physics: {
                    stabilization: true,
                    barnesHut: {
                        gravitationalConstant: -1000,      // Слабее отталкивание
                        centralGravity: 0.05,              // Ослабить притяжение к центру
                        springLength: 80,                  // Еще короче связи
                        springConstant: 0.04,
                        damping: 0.5,
                        avoidOverlap: 0.7   
                    }
                },
                height: "400px",
                locales:"ru"
            }
        }
    },
    mixins:[
        ChanelApiMixine
    ],
    components:{
        ViewModal,
        BaseModal,
        Network
    },
    methods:{
        async Open(id){

            this.$emit("StartLoading")

            var container = this.$refs.network_container

            this.options.height = container.height

            try{
                var relations = await this.GetSystemRelations()

                if(relations.code === 200){

                    this.viewedRelationsModel = relations.body;

                    var allNodes = []
                    var allEges = []

                    for (let i = 0; i < this.viewedRelationsModel.length; i++) {
                        const element = this.viewedRelationsModel[i];

                        allNodes.push(this.AddNode(element, id))

                        for (let j = 0; j < element.outputIds.length; j++) {
                            const egeDto = element.outputIds[j];
                            allEges.push(this.AddEge(egeDto, element))
                        }
                    }

                    this.nodes = allNodes
                    this.edges = allEges

                    console.log(this.edges)
                    console.log(this.nodes)
                }

                await this.$refs.baseModal.OpenModal()
            }
            finally{
                this.$emit("StopLoading")
            }
        },
        AddNode(element, currentId){
            var node = {
                id: element.objectId, 
                label: element.objectName,
                href: this.GetRelationUrl(element.isChanel, element.objectId)
            }

            if(!element.isChanel){
                node.shape = "ellipse"
            }

            var color = element.objectId == currentId 
                        ? this.GetColorFromClass("success") 
                        : (element.isChanel 
                            ? this.GetColorFromClass("info")
                            : this.GetColorFromClass("light")
                        )

            node.color = {
                background: color,
                highlight: {
                    background:color
                }
            } 
            return node
        },
        AddEge(egeDto, element){
            var ege = { 
                from: element.objectId, 
                to: egeDto.targetId 
            }

            var color = this.GetColorFromClass("danger");

            switch(egeDto.status){
                case 0:
                    color = this.GetColorFromClass("primary")
                    break;
                case 1:
                    color = this.GetColorFromClass("warning")
                    break;
            }

            ege.color = {
                color: color,
                highlight: color
            }        

            return ege
        },
        GetColorFromClass(colorName){
            const root = document.documentElement;

            return getComputedStyle(root)
                    .getPropertyValue(`--bs-${colorName}`)
                    .trim();
        },
        GetRelationUrl(isChanel, id){
            if(isChanel==1)
                return `/chanels?id=${id}`
            else
                return `/connections?id=${id}`
        },
        GetRowClass(row){
            if(row.status != 0)
                return "table-danger"
            else 
                return ""
        }
    },
    mounted(){
        var container = this.$refs.network_container

        var network = this.$refs.network

        network.once('afterDrawing', function () {
            var scale = 1.5;
            network.moveTo({
                offset: {
                    x: (0.5 * container.offsetWidth) * scale,
                    y: (0.5 * container.offsetHeight) * scale
                },
                scale: scale
            });
        });

        network.on('doubleClick', (opts) => {

            if(opts.nodes.length >0){
                var nodeId = opts.nodes[0]

                var node = network.nodes.find(x=> x.id == nodeId)

                window.open(node.href, '_blank');
            }

            //network.unselectAll();
        });
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
                Реальные связи
            </h5>
        </template>
        <template v-slot:body >
            <div class="d-flex flex-column" ref="network_container">
                <Network
                    ref="network"
                    :edges="this.edges"
                    :nodes="this.nodes"
                    :options="this.options"
                />

            </div>            
        </template>

        <template v-slot:footer>
        </template>
    </BaseModal>
</template>