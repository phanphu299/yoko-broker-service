FROM emqx/emqtt-bench:latest
COPY ["listeners/emqtt-bench/custom_eqmtt_bench.sh",  "custom_eqmtt_bench.sh"]

RUN chmod +x ./custom_eqmtt_bench.sh

ENTRYPOINT ["/bin/sh"]
