<template>
  <div>
    <h1>Additive {{id}}</h1>
    <p>Edit purchase additive batch</p>
    <router-link :to="{ path: `/purchase/additive/detail/${id}`}" exact>
      Back to Purchase Batch List
    </router-link>
  </div>
</template>

<script>
export default {
  name: 'AdditiveEdit',
  props: {
    id: {
      type: [Number, String],
      validator(value) {
        return Number.isInteger(Number(value));
      },
    },
  },
};
</script>
