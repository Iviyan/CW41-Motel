<template>
	<h1 class="fs-4 text-center">График аренды комнат</h1>

	<canvas ref="chartEl"></canvas>

	<router-link :to="{ name: 'lease-agreements' }" class="btn btn-outline-secondary mt-3">Назад</router-link>
</template>

<script setup>
import { Chart, registerables } from 'chart.js';
import {inject, onMounted, ref} from "vue";
import {call_get} from "@/utils/api";

Chart.register(...registerables);

const chartEl = ref(null);
let chart = null;

const rooms = ref([])
const leaseAgreements = inject('leaseAgreements')

onMounted(async () => {
	rooms.value = await call_get(`/api/rooms`)

	let leaseRooms = leaseAgreements.value.map(l => l.rooms).flat(1);
	let data = rooms.value.map(r => r.number).reduce((a, v) => ({ ...a, [v]: 0}), {});
	for (let r of leaseRooms) data[r]++;
	data = Object.keys(data).map(e => ({ x:e, y:data[e] }));
	console.log(data);

	chart = new Chart(chartEl.value.getContext("2d"), {
		type: 'bar',
		data: {
			datasets: [{
				data: data,
				label: 'Номера',
			}]
		},
		options: {
			responsive: true,
		}
	});
});
</script>
