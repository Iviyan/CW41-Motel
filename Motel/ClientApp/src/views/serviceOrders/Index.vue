<template>
	<loader :show="isLoading" />

	<router-view v-if="!isLoading" />
</template>

<script setup>
import {ref, onMounted, provide} from 'vue';
import { useRouter } from 'vue-router';

import Loader from "@/components/Loader";

import { call_get } from '@/utils/api';

const router = useRouter();

const serviceOrders = ref([]);
const services = ref([]);
const rooms = ref([]);
provide('serviceOrders', serviceOrders);
provide('services', services);
provide('rooms', rooms);

const isLoading = ref(true);

onMounted(async () => {
	isLoading.value = true;
	serviceOrders.value = await call_get(`/api/service-orders`, { verbose: true })
	services.value = await call_get(`/api/services`, { only_actual: true })
	rooms.value = await call_get(`/api/rooms`)
	isLoading.value = false;
});
</script>
