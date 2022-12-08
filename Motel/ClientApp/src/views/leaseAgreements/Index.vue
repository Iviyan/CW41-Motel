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

const leaseAgreements = ref([]);
const rooms = ref([]);
provide('leaseAgreements', leaseAgreements);
provide('rooms', rooms);

const isLoading = ref(true);

onMounted(async () => {
	isLoading.value = true;
	leaseAgreements.value = await call_get(`/api/lease-agreements`)
	rooms.value = await call_get(`/api/rooms`, { only_free: true })
	isLoading.value = false;
});
</script>
