#include "GENTPTypes.h"

#include <assert.h>
#include <math.h>
#include <stdlib.h>

ufixed32_t ge_ufixed32(uint16_t whole, uint16_t fraction) {
    return (struct ge_ufixed32) { .whole = whole, .fraction = fraction };
}

ufixed64_t ge_ufixed64(uint32_t whole, uint32_t fraction) {
    return (struct ge_ufixed64) { .whole = whole, .fraction = fraction };
}

double ge_ufixed64_as_double(ufixed64_t uf64) {
    return uf64.whole + uf64.fraction * pow(2, -32);
}

ufixed64_t ge_ufixed64_with_double(double value) {
    assert(value >= 0);
    return ge_ufixed64(value, (value - trunc(value) * pow(2, 32)));
}

ufixed32_t ge_hton_ufixed32(ufixed32_t uf32) {
    return ge_ufixed32(htons(uf32.whole), htons(uf32.fraction));
}
ufixed32_t ge_ntoh_ufixed32(ufixed32_t uf32) {
    return ge_ufixed32(ntohs(uf32.whole), ntohs(uf32.fraction));
}

ufixed64_t ge_hton_ufixed64(ufixed64_t uf64) {
    return ge_ufixed64(htonl(uf64.whole), htonl(uf64.fraction));
}
ufixed64_t ge_ntoh_ufixed64(ufixed64_t uf64) {
    return ge_ufixed64(ntohl(uf64.whole), ntohl(uf64.fraction));
}

ntp_packet_t ge_hton_ntp_packet(ntp_packet_t p) {
    p.root_delay = ge_hton_ufixed32(p.root_delay);
    p.root_dispersion = ge_hton_ufixed32(p.root_dispersion);
    
    p.reference_timestamp = ge_hton_ufixed64(p.reference_timestamp);
    p.originate_timestamp = ge_hton_ufixed64(p.originate_timestamp);
    p.receive_timestamp = ge_hton_ufixed64(p.receive_timestamp);
    p.transmit_timestamp = ge_hton_ufixed64(p.transmit_timestamp);
    
    return p;
}

ntp_packet_t ge_ntoh_ntp_packet(ntp_packet_t p) {
    p.root_delay = ge_ntoh_ufixed32(p.root_delay);
    p.root_dispersion = ge_ntoh_ufixed32(p.root_dispersion);
    
    p.reference_timestamp = ge_ntoh_ufixed64(p.reference_timestamp);
    p.originate_timestamp = ge_ntoh_ufixed64(p.originate_timestamp);
    p.receive_timestamp = ge_ntoh_ufixed64(p.receive_timestamp);
    p.transmit_timestamp = ge_ntoh_ufixed64(p.transmit_timestamp);
    
    return p;
}

