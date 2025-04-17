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
            nodes: [
                { id: 1, label: 'Node 1' },
                { id: 2, label: 'Node 2' },
                { id: 3, label: 'Node 3' }
            ],
            edges: [
                { from: 1, to: 2, label: 'Relation' },
                { from: 2, to: 3, arrows: 'to' },
                { from: 3, to: 1 }
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
                        avoidOverlap: 0.5   
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
            var relations = await this.GetSystemRelations()

            if(relations.code === 200){

                this.viewedRelationsModel = relations.body;

                var idDict = {}
                var lastId = 0;

                function GetIdByGuid(guid){
                    if(idDict[guid] != null && idDict[guid] != undefined)
                        return idDict[guid]
                    lastId++;
                    idDict[guid] = lastId
                    return lastId
                }

                this.nodes = []
                this.edges = []

                var allNodes = []
                var allEges = []

                for (let index = 0; index < this.viewedRelationsModel.length; index++) {
                    const element = this.viewedRelationsModel[index];

                    var node = {
                         id: element.objectId, 
                         label: element.objectName,
                        }

                    if(!element.isChanel){
                        node.shape = "ellipse"
                    }

                    node.color= {
                            background: element.objectId == id ? "#198754": '#D2E5FF'
                        } 


                    allNodes.push(node)

                    for (let j = 0; j < element.outputIds.length; j++) {
                        const egeDto = element.outputIds[j];

                        var ege = { 
                            from: element.objectId, 
                            to: egeDto.targetId 
                        }

                        ege.color = {
                                color: egeDto.status != 0 ? "#dc3545": '#848484'
                            }


                        allEges.push(ege)
                    }
                }

                this.nodes = allNodes
                this.edges = allEges

                console.log(this.edges)
                console.log(this.nodes)
            }

            await this.$refs.baseModal.OpenModal()
        },
        GetRelationUrl(row){
            if(row.direction==1)
                return `/chanels?id=${row.relationId}`
            else
                return `/connections?id=${row.relationId}`
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
            // Assuming 'container' is the DOM element where the network is rendered
            var containerWidth = container.offsetWidth;
            var containerHeight = container.offsetHeight;
            var scale = 1.5;
            console.log(containerWidth, containerHeight)
            network.moveTo({
                offset: {
                    x: (0.5 * containerWidth) * scale,
                    y: (0.5 * containerHeight) * scale
                },
                scale: scale
            });
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




<!-- <ViewModal

    entityName=""
    :struncture="this.relationViewStruecture"
    v-model:viewedEntity="this.viewedRelationsModel"
/> -->

</template>