<!-- eslint-disable -->
<template>
  <div>
    <div class="row">
        <div class="panel panel-default">
            <div class="panel-body">
                <form class="form-inline">
                    <div class="form-group">
                        <label class="pull-left" for="ReportingYear" style="padding-left: 10pt; padding-right: 10pt">Reporting Year</label>
                        <div id="ReportingYear"></div>
                    </div>
                    <div class="form-group">
                        <label class="pull-left" for="ReportingMonth" style="padding-left: 10pt; padding-right: 10pt">Reporting Month</label>
                        <div id="ReportingMonth"></div>
                    </div>
                    <input v-on:click="getReport" type="button" value="Generate Report" id="GenerateReportBtn" />
                </form>
            </div>
        </div>
    </div>
    <p class="s2" style="padding-top: 4pt;padding-bottom: 1pt;padding-left: 977pt;text-indent: 0pt;text-align: left;">OMB No. 1513-0047 (11/30/2012)</p>
    <table style="border-collapse:collapse;margin-left:5.49pt" cellspacing="0">
        <tr style="height:11pt">
            <td style="width:354pt;border-top-style:solid;border-top-width:2pt;border-top-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s3" style="padding-left: 93pt;text-indent: 0pt;line-height: 10pt;text-align: left;">DEPARTMENT OF THE TREASURY</p>
            </td>
            <td style="width:481pt;border-top-style:solid;border-top-width:2pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-right-style:solid;border-right-width:1pt">
                <p class="s4" style="padding-left: 1pt;text-indent: 0pt;line-height: 9pt;text-align: left;">1. NAME OF PROPRIETOR<div v-if="productionRecords" id="h_NameofProprietor">{{ reportHeader.ProprietorName }}</div></p>
            </td>
            <td style="width:107pt;border-top-style:solid;border-top-width:2pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s5" style="padding-left: 3pt;text-indent: 0pt;line-height: 9pt;text-align: left;">2. EIN<div v-if="productionRecords" id="h_EIN">{{ reportHeader.EIN }}</div></p>
            </td>
            <td style="width:156pt;border-top-style:solid;border-top-width:2pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20">
                <p class="s4" style="padding-left: 2pt;text-indent: 0pt;line-height: 8pt;text-align: left;">3. MONTH AND YEAR<div v-if="productionRecords" id="h_MonthAndYear">{{ reportHeader.ReportDate }}</div></p>
            </td>
        </tr>
        <tr style="height:9pt">
            <td style="width:354pt;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s6" style="padding-left: 61pt;text-indent: 0pt;line-height: 8pt;text-align: left;">ALCOHOL AND TOBACCO TAX AND TRADE BUREAU (TTB)</p>
            </td>
            <td style="width:481pt;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-right-style:solid;border-right-width:1pt">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
            </td>
            <td style="width:107pt;border-left-style:solid;border-left-width:1pt;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
            </td>
            <td style="width:156pt;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
            </td>
        </tr>
        <tr style="height:1pt">
            <td style="width:354pt;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p class="s7" style="padding-left: 27pt;text-indent: 0pt;line-height: 12pt;text-align: left;">MONTHLY REPORT OF PRODUCTION OPERATIONS</p>
            </td>
            <td style="width:588pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="2" rowspan="3">
                <p class="s4" style="padding-left: 1pt;text-indent: 0pt;text-align: left;">4. LOCATION OF PLANT<div v-if="productionRecords" id="h_LocationOfPlant">{{ reportHeader.PlantAddress }}</div></p>
            </td>
            <td style="width:156pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20" rowspan="3">
                <p class="s4" style="padding-left: 2pt;text-indent: 0pt;text-align: left;">5. PLANT NUMBER</p>
                <p class="s6" style="padding-left: 14pt;text-indent: 0pt;text-align: left;"><div v-if="productionRecords" id="h_PlantNumber">{{ reportHeader.DSP }}</div></p>
            </td>
        </tr>
        <tr style="height:11pt" />
        <tr style="height:9pt">
            <td style="width:354pt;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s5" style="padding-left: 60pt;text-indent: 0pt;line-height: 9pt;text-align: left;">(PREPARE IN DUPLICATE. SEE INSTRUCTIONS BELOW)</p>
            </td>
        </tr>
    </table>
    <h1 style="padding-top: 1pt;padding-bottom: 1pt;text-indent: 0pt;text-align: center;">PART I - TRANSACTIONS</h1>
    <table id="tablePart1" style="border-collapse:collapse;margin-left:5.88pt" cellspacing="0">
        <tr style="height:11pt">
            <td style="width:234pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="2" rowspan="3">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
                <p class="s4" style="padding-top: 6pt;padding-left: 34pt;padding-right: 34pt;text-indent: 0pt;text-align: center;">TRANSACTION</p>
                <p class="s4" style="padding-left: 34pt;padding-right: 34pt;text-indent: 0pt;text-align: center;">(a)</p>
            </td>
            <td style="width:864pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20" colspan="11">
                <p class="s4" style="padding-top: 1pt;padding-left: 324pt;padding-right: 468pt;text-indent: 0pt;text-align: center;">PROOF GALLONS</p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:174pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="2">
                <p class="s4" style="padding-left: 70pt;padding-right: 69pt;text-indent: 0pt;text-align: center;">WHISKY</p>
            </td>
            <td style="width:144pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="2">
                <p class="s4" style="padding-left: 54pt;padding-right: 54pt;text-indent: 0pt;text-align: center;">BRANDY</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p class="s4" style="padding-top: 5pt;padding-left: 14pt;padding-right: 14pt;text-indent: 0pt;text-align: center;">RUM</p>
                <p class="s4" style="padding-left: 14pt;padding-right: 14pt;text-indent: 0pt;text-align: center;">(f)</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p class="s4" style="padding-top: 5pt;padding-left: 27pt;padding-right: 27pt;text-indent: 0pt;text-align: center;">GIN</p>
                <p class="s4" style="padding-left: 27pt;padding-right: 27pt;text-indent: 0pt;text-align: center;">(g)</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p class="s4" style="padding-top: 5pt;padding-left: 1pt;padding-right: 1pt;text-indent: 0pt;text-align: center;">VODKA</p>
                <p class="s4" style="padding-left: 1pt;padding-right: 1pt;text-indent: 0pt;text-align: center;">(h)</p>
            </td>
            <td style="width:174pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="2">
                <p class="s4" style="padding-left: 41pt;text-indent: 0pt;text-align: left;">ALCOHOL AND SPIRITS</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p class="s4" style="padding-left: 1pt;text-indent: 0pt;text-align: center;">OTHER</p>
                <p class="s4" style="padding-left: 1pt;padding-right: 1pt;text-indent: 0pt;text-align: center;">(Identify)<br />(k)</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20" rowspan="2">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
                <p class="s4" style="padding-left: 27pt;padding-right: 28pt;text-indent: 0pt;text-align: center;">TOTAL</p>
                <p class="s4" style="padding-left: 27pt;padding-right: 28pt;text-indent: 0pt;text-align: center;">(l)</p>
            </td>
        </tr>
        <tr style="height:24pt">
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 7pt;padding-right: 8pt;text-indent: 0pt;text-align: center;">160<span class="s9"></span><span class="s10"> </span>AND UNDER</p>
                <p class="s4" style="padding-left: 7pt;padding-right: 8pt;text-indent: 0pt;text-align: center;">(b)</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 23pt;padding-right: 23pt;text-indent: 0pt;text-align: center;">OVER 160<span class="s9"></span></p>
                <p class="s4" style="padding-left: 23pt;padding-right: 22pt;text-indent: 0pt;text-align: center;">(c)</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 1pt;padding-right: 2pt;text-indent: 0pt;text-align: center;">170<span class="s9"></span><span class="s10"> </span>AND UNDER</p>
                <p class="s4" style="padding-left: 1pt;padding-right: 6pt;text-indent: 0pt;text-align: center;">(d)</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-left: 14pt;padding-right: 14pt;text-indent: 0pt;line-height: 10pt;text-align: center;">OVER 170<span class="s9"></span></p>
                <p class="s4" style="padding-left: 14pt;padding-right: 14pt;text-indent: 0pt;text-align: center;">(e)</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="line-height: 9pt;text-align: center;">190<span class="s9"></span><span class="s10"> </span>AND OVER</p>
                <p class="s11" style="padding-top: 1pt;line-height: 7pt;text-align: center;">(Distillation Proof)<br /> (i)</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="line-height: 8pt;text-align: center;">UNDER 190<span class="s9"></span></p>
                <p class="s11" style="padding-top: 1pt;line-height: 7pt;text-align: center;">(Distillation Proof)<br /> (j)</p>
            </td>
        </tr>
        <!-- PART I - TRANSACTIONS -->
        <tr style="height:11pt">
            <td style="width:12pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="8">
                <p class="s4" style="padding-top: 1pt;padding-left: 13pt;text-indent: 0pt;text-align: left;">WITHDRAWN FOR</p>
            </td>
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 2pt;text-indent: 0pt;text-align: left;">1. TAX PAYMENT</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_1b_WhiskyUnder160"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_1c_WhiskyOver160"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_1d_BrandyUnder170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_1e_BrandyOver170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_1f_Rum"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_1g_Gin"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_1h_Vodka"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_1i_AlcoholAndSpiritsOver190"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_1j_AlcoholAndSpiritsUnder190"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_1k_Other"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_1l_Total"></div></p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 2pt;text-indent: 0pt;line-height: 9pt;text-align: left;">2. USE OF U.S.</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_2b_WhiskyUnder160"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_2c_WhiskyOver160"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_2d_BrandyUnder170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_2e_BrandyOver170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_2f_Rum"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_2g_Gin"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_2h_Vodka"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_2i_AlcoholAndSpiritsOver190"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_2j_AlcoholAndSpiritsUnder190"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_2k_Other"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_2l_Total"></div></p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-left: 2pt;text-indent: 0pt;text-align: left;">3. HOSPITAL, SCIENTIFIC, OR EDUCATIONAL USE <span class="s12">1</span></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_3b_WhiskyUnder160"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_3c_WhiskyOver160"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_3d_BrandyUnder170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_3e_BrandyOver170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_3f_Rum"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_3g_Gin"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_3h_Vodka"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_3i_AlcoholAndSpiritsOver190"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_3j_AlcoholAndSpiritsUnder190"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_3k_Other"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_3l_Total"></div></p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-left: 2pt;text-indent: 0pt;text-align: left;">4. EXPORT</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_4b_WhiskyUnder160"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_4c_WhiskyOver160"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_4d_BrandyUnder170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_4e_BrandyOver170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_4f_Rum"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_4g_Gin"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_4h_Vodka"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_4i_AlcoholAndSpiritsOver190"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_4j_AlcoholAndSpiritsUnder190"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_4k_Other"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_4l_Total"></div></p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-left: 2pt;text-indent: 0pt;text-align: left;">5. TRANSFER TO FOREIGN TRADE ZONE</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_5b_WhiskyUnder160"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_5c_WhiskyOver160"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_5d_BrandyUnder170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_5e_BrandyOver170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_5f_Rum"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_5g_Gin"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_5h_Vodka"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_5i_AlcoholAndSpiritsOver190"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_5j_AlcoholAndSpiritsUnder190"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_5k_Other"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_5l_Total"></div></p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-left: 2pt;text-indent: 0pt;text-align: left;">6. TRANSFER TO CMBW</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_6b_WhiskyUnder160"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_6c_WhiskyOver160"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_6d_BrandyUnder170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_6e_BrandyOver170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_6f_Rum"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_6g_Gin"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_6h_Vodka"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_6i_AlcoholAndSpiritsOver190"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_6j_AlcoholAndSpiritsUnder190"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_6k_Other"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_6l_Total"></div></p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-left: 2pt;text-indent: 0pt;text-align: left;">7. USE AS SUPPLIES ON VESSELS OR AIRCRAFT</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_7b_WhiskyUnder160"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_7c_WhiskyOver160"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_7d_BrandyUnder170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_7e_BrandyOver170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_7f_Rum"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_7g_Gin"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_7h_Vodka"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_7i_AlcoholAndSpiritsOver190"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_7j_AlcoholAndSpiritsUnder190"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_7k_Other"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_7l_Total"></div></p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 2pt;text-indent: 0pt;text-align: left;">8. USE IN WINE PRODUCTION</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_8b_WhiskyUnder160"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_8c_WhiskyOver160"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_8d_BrandyUnder170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_8e_BrandyOver170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_8f_Rum"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_8g_Gin"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_8h_Vodka"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_8i_AlcoholAndSpiritsOver190"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_8j_AlcoholAndSpiritsUnder190"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_8k_Other"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_8l_Total"></div></p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:12pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
            </td>
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-left: 2pt;text-indent: 0pt;text-align: left;">9. ENTERED IN PROCESSING ACCOUNT</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_9b_WhiskyUnder160" readonly=readonly />{{ whiskyUnder160 ? whiskyUnder160.ProccessingAcct : '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_9c_WhiskyOver160" readonly=readonly />{{ whiskyOver160 ? whiskyOver160.ProccessingAcct: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_9d_BrandyUnder170" readonly=readonly />{{ brandyUnder170 ? brandyUnder170.ProccessingAcct: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_9e_BrandyOver170" readonly=readonly />{{ brandyOver170 ? brandyOver170.ProccessingAcct: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_9f_Rum" readonly=readonly />{{ rum ? rum.ProccessingAcct: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_9g_Gin" readonly=readonly />{{ gin ? gin.ProccessingAcct: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_9h_Vodka" readonly=readonly />{{ vodka ? vodka.ProccessingAcct: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_9i_AlcoholAndSpiritsOver190" readonly=readonly />{{ alcoholOver190 ? alcoholOver190.ProccessingAcct: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_9j_AlcoholAndSpiritsUnder190" readonly=readonly />{{ alcoholUnder190 ? alcoholUnder190.ProccessingAcct: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_9k_Other" readonly=readonly />{{ other ? other.ProccessingAcct: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_9l_Total" readonly=readonly />{{ total ? total.ProccessingAcct: '' }}</p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:12pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
            </td>
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-left: 2pt;text-indent: 0pt;text-align: left;">10. ENTERED FOR TRANSFER IN BOND</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_10b_WhiskyUnder160"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_10c_WhiskyOver160"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_10d_BrandyUnder170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_10e_BrandyOver170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_10f_Rum"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_10g_Gin"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_10h_Vodka"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_10i_AlcoholAndSpiritsOver190"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_10j_AlcoholAndSpiritsUnder190"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_10k_Other"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_10l_Total"></div></p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:12pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_1k_Other"></div></p>
            </td>
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-left: 2pt;text-indent: 0pt;text-align: left;">11. ENTERED IN STORAGE ACCOUNT</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_11b_WhiskyUnder160" readonly=readonly />{{ whiskyUnder160 ? whiskyUnder160.StorageAcct: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_11c_WhiskyOver160" readonly=readonly /> {{ whiskyOver160 ? whiskyOver160.StorageAcct: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_11d_BrandyUnder170" readonly=readonly />{{ brandyUnder170 ? brandyUnder170.StorageAcct: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_11e_BrandyOver170" readonly=readonly />{{ brandyOver170 ? brandyOver170.StorageAcct: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_11f_Rum" readonly=readonly />{{ rum ? rum.StorageAcct: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_11g_Gin" readonly=readonly />{{ gin ? gin.StorageAcct: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_11h_Vodka" readonly=readonly />{{ vodka ? vodka.StorageAcct: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_11i_AlcoholAndSpiritsOver190" readonly=readonly />{{ alcoholOver190 ? alcoholOver190.StorageAcct: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_11j_AlcoholAndSpiritsUnder190" readonly=readonly />{{ alcoholUnder190 ? alcoholUnder190.StorageAcct: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_11k_Other" readonly=readonly />{{ other ? other.StorageAcct: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_11l_Total" readonly=readonly />{{ total ? total.StorageAcct: '' }}</p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:12pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
            </td>
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 17pt;padding-right: 10pt;text-indent: -15pt;text-align: left;">12. WITHDRAWN FOR RESEARCH, DEVELOPMENT, OR TESTING<span class="s12">2</span></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_12b_WhiskyUnder160"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_12c_WhiskyOver160"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_12d_BrandyUnder170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_12e_BrandyOver170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_12f_Rum"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_12g_Gin"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_12h_Vodka"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_12i_AlcoholAndSpiritsOver190"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_12j_AlcoholAndSpiritsUnder190"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_12k_Other"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_12l_Total"></div></p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:12pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
            </td>
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-left: 2pt;text-indent: 0pt;text-align: left;">13.</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_13b_WhiskyUnder160"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_13c_WhiskyOver160"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_13d_BrandyUnder170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_13e_BrandyOver170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_13f_Rum"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_13g_Gin"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_13h_Vodka"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_13i_AlcoholAndSpiritsOver190"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_13j_AlcoholAndSpiritsUnder190"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_13k_Other"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_13l_Total"></div></p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:12pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
            </td>
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-left: 2pt;text-indent: 0pt;text-align: left;">14. PRODUCED (Total, Lines 1 through 13)</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_14b_WhiskyUnder160" readonly=readonly />{{ whiskyUnder160 ? whiskyUnder160.ProducedTotal: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords"  style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_14c_WhiskyOver160" readonly=readonly />{{ whiskyOver160 ? whiskyOver160.ProducedTotal: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_14d_BrandyUnder170" readonly=readonly />{{ brandyUnder170 ? brandyUnder170.ProducedTotal: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_14e_BrandyOver170" readonly=readonly />{{ brandyOver170 ? brandyOver170.ProducedTotal: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_14f_Rum" readonly=readonly />{{ rum ? rum.ProducedTotal: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_14g_Gin" readonly=readonly />{{ gin ? gin.ProducedTotal: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_14h_Vodka" readonly=readonly />{{ vodka ? vodka.ProducedTotal: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_14i_AlcoholAndSpiritsOver190" readonly=readonly />{{ alcoholOver190 ? alcoholOver190.ProducedTotal: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_14j_AlcoholAndSpiritsUnder190" readonly=readonly />{{ alcoholUnder190 ? alcoholUnder190.ProducedTotal: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_14k_Other" readonly=readonly />{{ other ? other.ProducedTotal: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_14l_Total" readonly=readonly />{{ total ? total.ProducedTotal: '' }}</p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:12pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
            </td>
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-left: 2pt;text-indent: 0pt;text-align: left;">15. RECEIVED FOR REDISTILLATION <span class="s12">3</span></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_15b_WhiskyUnder160" readonly=readonly /> {{ whiskyUnder160 ? whiskyUnder160.Recd4RedistilaltionL15: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_15c_WhiskyOver160" readonly=readonly />{{ whiskyOver160 ? whiskyOver160.Recd4RedistilaltionL15: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_15d_BrandyUnder170" readonly=readonly />{{ brandyUnder170 ? brandyUnder170.Recd4RedistilaltionL15: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_15e_BrandyOver170" readonly=readonly />{{ brandyOver170 ? brandyOver170.Recd4RedistilaltionL15: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_15f_Rum" readonly=readonly />{{ rum ? rum.Recd4RedistilaltionL15: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_15g_Gin" readonly=readonly />{{ gin ? gin.Recd4RedistilaltionL15: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_15h_Vodka" readonly=readonly />{{ vodka ? vodka.Recd4RedistilaltionL15: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_15i_AlcoholAndSpiritsOver190" readonly=readonly />{{ alcoholOver190 ? alcoholOver190.Recd4RedistilaltionL15: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_15j_AlcoholAndSpiritsUnder190" readonly=readonly />{{ alcoholUnder190 ? alcoholUnder190.Recd4RedistilaltionL15: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_15k_Other" readonly=readonly />{{ other ? other.Recd4RedistilaltionL15: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_15l_Total" readonly=readonly />{{ total ? total.Recd4RedistilaltionL15: '' }}</p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:12pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
            </td>
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 2pt;text-indent: 0pt;text-align: left;">16. <span class="s12">4</span></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_16b_WhiskyUnder160"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_16c_WhiskyOver160"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_16d_BrandyUnder170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_16e_BrandyOver170"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_16f_Rum"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_16g_Gin"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_16h_Vodka"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_16i_AlcoholAndSpiritsOver190"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_16j_AlcoholAndSpiritsUnder190"></div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_17k_Other"></div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p1_17l_Total"></div></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p class="s4" style="padding-top: 1pt;padding-left: 14pt;text-indent: -15pt;text-align: left;">17. PHYSICAL INVENTORY- END OF CALENDAR QUARTER</p>
            </td>
            <td style="width:144pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 3pt;padding-left: 17pt;padding-right: 57pt;text-indent: -15pt;line-height: 10pt;text-align: left;">(a) RECEIVED FOR REDISTILLATION<span class="s12">3</span></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div v-if="productionRecords" id="p1_17a_b_WhiskyUnder160">{{ whiskyUnder160 ? whiskyUnder160.Recd4RedistilL17: '' }}</div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div v-if="productionRecords" id="p1_17a_c_WhiskyOver160">{{ whiskyOver160 ? whiskyOver160.Recd4RedistilL17: '' }}</div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div v-if="productionRecords" id="p1_17a_d_BrandyUnder170">{{ brandyUnder170 ? brandyUnder170.Recd4RedistilL17: '' }}</div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div v-if="productionRecords" id="p1_17a_e_BrandyOver170">{{ brandyOver170 ? brandyOver170.Recd4RedistilL17: '' }}</div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div div v-if="productionRecords" id="p1_17a_f_Rum">{{ rum ? rum.Recd4RedistilL17: '' }}</div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div div v-if="productionRecords" id="p1_17a_g_Gin">{{ gin ? gin.Recd4RedistilL17: '' }}</div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div div v-if="productionRecords" id="p1_17a_h_Vodka">{{ vodka ? vodka.Recd4RedistilL17: '' }}</div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div div v-if="productionRecords" id="p1_17a_i_AlcoholAndSpiritsOver190">{{ alcoholOver190 ? alcoholOver190.Recd4RedistilL17: '' }}</div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div div v-if="productionRecords" id="p1_17a_j_AlcoholAndSpiritsUnder190">{{ alcoholUnder190 ? alcoholUnder190.Recd4RedistilL17: '' }}</div></p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div div v-if="productionRecords" id="p1_17a_k_Other">{{ other ? other.Recd4RedistilL17: '' }}</div></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div div v-if="productionRecords" id="p1_17a_l_Total">{{ total ? total.Recd4RedistilL17: '' }}</div></p>
            </td>
        </tr>
        <tr style="height:10pt">
            <td style="width:144pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-left: 2pt;text-indent: 0pt;line-height: 9pt;text-align: left;">(b) UNFINISHED SPIRITS</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_17b_b_WhiskyUnder160" readonly=readonly />{{ whiskyUnder160 ? whiskyUnder160.UnfinishedSpiritsEndOfQuarter: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_17b_c_WhiskyOver160" readonly=readonly />{{ whiskyOver160 ? whiskyOver160.UnfinishedSpiritsEndOfQuarter: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_17b_d_BrandyUnder170" readonly=readonly />{{ brandyUnder170 ? brandyUnder170.UnfinishedSpiritsEndOfQuarter: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_17b_e_BrandyOver170" readonly=readonly />{{ brandyOver170 ? brandyOver170.UnfinishedSpiritsEndOfQuarter: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_17b_f_Rum" readonly=readonly />{{ rum ? rum.UnfinishedSpiritsEndOfQuarter: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_17b_g_Gin" readonly=readonly />{{ gin ? gin.UnfinishedSpiritsEndOfQuarter: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_17b_h_Vodka" readonly=readonly />{{ vodka ? vodka.UnfinishedSpiritsEndOfQuarter: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_17b_i_AlcoholAndSpiritsOver190" readonly=readonly />{{ alcoholOver190 ? alcoholOver190.UnfinishedSpiritsEndOfQuarter: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_17b_j_AlcoholAndSpiritsUnder190" readonly=readonly />{{ alcoholUnder190 ? alcoholUnder190.UnfinishedSpiritsEndOfQuarter: '' }}</p>
            </td>
            <td style="width:72pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_17b_k_Other" readonly=readonly />{{ other ? other.UnfinishedSpiritsEndOfQuarter: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:2pt;border-bottom-color:#231F20">
                <p v-if="productionRecords" style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p1_17b_l_Total" readonly=readonly />{{ total ? total.UnfinishedSpiritsEndOfQuarter: '' }}</p>
            </td>
        </tr>
        <!-- End PART I - TRANSACTIONS-->
        <tr style="height:35pt">
            <td style="width:234pt;border-top-style:solid;border-top-width:2pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="2">
                <p class="s6" style="padding-top: 4pt;padding-left: 34pt;padding-right: 34pt;text-indent: 0pt;line-height: 89%;text-align: center;">PART II - PRODUCTION OF ALCOHOL AND SPIRITS OF 190<span class="s13"></span><span class="s14"> </span>OR MORE OF PROOF,</p>
                <p class="s6" style="padding-left: 34pt;padding-right: 34pt;text-indent: 0pt;text-align: center;">BY KIND OF MATERIAL USED</p>
            </td>
            <td style="width:354pt;border-top-style:solid;border-top-width:2pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="4">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
                <p class="s6" style="padding-left: 40pt;text-indent: 0pt;text-align: left;">PART III - PRODUCTION OF WHISKY BY KIND AND COOPERAGE USED</p>
            </td>
            <td style="width:300pt;border-top-style:solid;border-top-width:2pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="4">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
                <p class="s6" style="padding-left: 61pt;text-indent: 0pt;text-align: left;">PART IV - PRODUCTION OF BRANDY BY KIND</p>
            </td>
            <td style="width:210pt;border-top-style:solid;border-top-width:2pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20" colspan="2">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
                <p class="s6" style="padding-left: 36pt;text-indent: 0pt;text-align: left;">PART V - USED IN REDISTILLATION</p>
            </td>
        </tr>
        <tr style="height:35pt">
            <td style="width:144pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
                <p class="s4" style="text-align: center;">KIND OF MATERIAL<br />(a)</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 7pt;padding-left: 9pt;padding-right: 9pt;text-indent: 0pt;text-align: center;">PROOF GALLONS</p>
                <p class="s4" style="padding-left: 9pt;padding-right: 9pt;text-indent: 0pt;text-align: center;">(b)</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
                <p class="s4" style="padding-left: 1pt;padding-right: 8pt;text-indent: 0pt;text-align: center;">KIND</p>
                <p class="s4" style="padding-left: 1pt;padding-right: 8pt;text-indent: 0pt;text-align: center;">(a)</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 4pt;padding-left: 8pt;padding-right: 8pt;text-indent: 0pt;text-align: center;">PROOF GALLONS NEW COOPERAGE</p>
                <p class="s4" style="padding-left: 23pt;padding-right: 23pt;text-indent: 0pt;text-align: center;">(b)</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 3pt;padding-left: 12pt;text-indent: 3pt;text-align: left;">PROOF GALLONS USED COOPERAGE</p>
                <p class="s4" style="padding-left: 39pt;padding-right: 39pt;text-indent: 0pt;text-align: center;">(c)</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 3pt;padding-left: 9pt;padding-right: 4pt;text-indent: 0pt;text-align: center;">PROOF GALLONS DEPOSITED IN TANKS</p>
                <p class="s4" style="padding-left: 9pt;padding-right: 9pt;text-indent: 0pt;text-align: center;">(d)</p>
            </td>
            <td style="width:198pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="3">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
                <p class="s4" style="padding-left: 88pt;padding-right: 88pt;text-indent: 0pt;text-align: center;">KIND</p>
                <p class="s4" style="padding-left: 88pt;padding-right: 88pt;text-indent: 0pt;text-align: center;">(a)</p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 8pt;padding-left: 18pt;padding-right: 18pt;text-indent: 0pt;text-align: center;">PROOF GALLON</p>
                <p class="s4" style="padding-left: 18pt;padding-right: 18pt;text-indent: 0pt;text-align: center;">(b)</p>
            </td>
            <td style="width:108pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 8pt;padding-left: 18pt;padding-right: 18pt;text-indent: 0pt;text-align: center;">KIND OF SPIRITS<span class="s12">5</span></p>
                <p class="s4" style="padding-left: 18pt;padding-right: 18pt;text-indent: 0pt;text-align: center;">(a)</p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p class="s4" style="padding-top: 7pt;padding-left: 18pt;padding-right: 18pt;text-indent: 0pt;text-align: center;">PROOF GALLON</p>
                <p class="s4" style="padding-left: 18pt;padding-right: 18pt;text-indent: 0pt;text-align: center;">(b)</p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:144pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 3pt;text-indent: 0pt;line-height: 9pt;text-align: left;">1. GRAIN</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p2_1b_GrainProof" style="text-indent: 0pt;text-align: left;">{{ part2 ? part2.grain: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 2pt;padding-left: 2pt;text-indent: 0pt;line-height: 8pt;text-align: left;">1. BOURBON</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p3_1b_BourbonProofNewCooperage" style="text-indent: 0pt;text-align: left;">{{ part3 ? part3.bourbonNewCooperage: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p3_1c_BourbonProofUsedCooperage" style="text-indent: 0pt;text-align: left;">{{ part3 ? part3.bourbonUsedCooperage: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p3_1d_BourbonProofDepositedInTanks" style="text-indent: 0pt;text-align: left;">{{ part3 ? part3.bourbonDepositedInTanks: '' }}</p>
            </td>
            <td style="width:198pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="3">
                <p class="s4" style="padding-left: 1pt;text-indent: 0pt;text-align: left;">1. GRAPE BRANDY</p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p4_1b_GrapeBrandyProof" style="text-indent: 0pt;text-align: left;">{{ part4 ? part4.grapeBrandy: '' }}</p>
            </td>
            <td style="width:108pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p5_1a_KindOfSpirits" class="s4" style="padding-top: 1pt;padding-left: 2pt;text-indent: 0pt;text-align: left;">1.{{ part5 ? part5.line_1.spirit: '' }}</p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p v-if="productionRecords" id="p5_1b_Proof" style="text-indent: 0pt;text-align: left;">{{ part5 ? part5.line_1.value: '' }}</p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:144pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 3pt;text-indent: 0pt;line-height: 9pt;text-align: left;">2. FRUIT</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p2_2b_FruitProof" style="text-indent: 0pt;text-align: left;">{{ part2 ? part2.fruit: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 2pt;padding-left: 2pt;text-indent: 0pt;line-height: 8pt;text-align: left;">2. CORN</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p3_2b_CornProofNewCooperage" style="text-indent: 0pt;text-align: left;">{{ part3 ? part3.cornNewCooperage: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p3_2c_CornProofUsedCooperage" style="text-indent: 0pt;text-align: left;">{{ part3 ? part3.cornUsedCooperage: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p3_2d_CornProofDepositedInTanks" style="text-indent: 0pt;text-align: left;">{{ part3 ? part3.cornDepositedInTanks: '' }}</p>
            </td>
            <td style="width:198pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="3">
                <p class="s4" style="padding-left: 1pt;text-indent: 0pt;text-align: left;">2. ALL OTHER BRANDY</p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p4_2b_AllOtherBrandyProof" style="text-indent: 0pt;text-align: left;">{{ part4 ? part4.allOtherBrandy: '' }}</p>
            </td>
            <td style="width:108pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" class="s4" id="p5_2a_KindOfSpirits" style="padding-top: 1pt;padding-left: 2pt;text-indent: 0pt;text-align: left;">2.{{ part5 ? part5.line_2.spirit: '' }}</p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p v-if="productionRecords" id="p5_2b_Proof" style="text-indent: 0pt;text-align: left;">{{ part5 ? part5.line_2.value: '' }}</p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:144pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 3pt;text-indent: 0pt;text-align: left;">3. MOLASSES</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p2_3b_MolassesProof" style="text-indent: 0pt;text-align: left;">{{ part2 ? part2.molasses: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 2pt;text-indent: 0pt;line-height: 9pt;text-align: left;">3. RYE</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p3_3b_RyeProofNewCoooperage" style="text-indent: 0pt;text-align: left;">{{ part3 ? part3.ryeNewCooperage: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p3_3c_RyeProofUsedCooperage" style="text-indent: 0pt;text-align: left;">{{ part3 ? part3.ryeUsedCooperage: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p3_3d_RyeProofDepositedInTanks" style="text-indent: 0pt;text-align: left;">{{ part3 ? part3.ryeDepositedInTanks: '' }}</p>
            </td>
            <td style="width:198pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="3">
                <p class="s4" style="padding-left: 1pt;text-indent: 0pt;text-align: left;">3. NEUTRAL GRAPE BRANDY</p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p4_3b_NeutralGrapeBrandyProof" style="text-indent: 0pt;text-align: left;">{{ part4 ? part4.neutralGrapeBrandy: '' }}</p>
            </td>
            <td style="width:108pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" class="s4" id="p5_3a_KindOfSpirits" style="padding-left: 2pt;text-indent: 0pt;text-align: left;">3.{{ part5 ? part5.line_3.spirit: '' }}</p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p v-if="productionRecords" id="p5_3b_Proof" style="text-indent: 0pt;text-align: left;">{{ part5 ? part5.line_3.value: '' }}</p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:144pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 3pt;text-indent: 0pt;line-height: 9pt;text-align: left;">4. ETHYL SULFATE</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p2_4b_EthylSulfateProof" style="text-indent: 0pt;text-align: left;">{{ part2 ? part2.ethylSulfate: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 2pt;padding-left: 2pt;text-indent: 0pt;line-height: 8pt;text-align: left;">4. LIGHT</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p3_4b_LightProofNewCooperage" style="text-indent: 0pt;text-align: left;">{{ part3 ? part3.lightNewCooperage: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p3_4c_LightProofUsedCooperage" style="text-indent: 0pt;text-align: left;">{{ part3 ? part3.lightUsedCooperage: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p3_4d_LightProofDepositedInTanks" style="text-indent: 0pt;text-align: left;">{{ part3 ? part3.lightDepositedInTanks: '' }}</p>
            </td>
            <td style="width:198pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="3">
                <p class="s4" style="padding-left: 1pt;text-indent: 0pt;text-align: left;">4. ALL OTHER NEUTRAL BRANDY</p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p4_4b_AllOtherNeutralBrandyProof" style="text-indent: 0pt;text-align: left;">{{ part4 ? part4.allOtherNeutralBrandy: '' }}</p>
            </td>
            <td style="width:108pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" v-if="productionRecords" id="p5_4a_KindOfSpirits"  style="padding-top: 1pt;padding-left: 2pt;text-indent: 0pt;text-align: left;">4.{{ part5 ? part5.line_4.spirit: '' }}</p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p v-if="productionRecords" id="p5_4b_Proof" style="text-indent: 0pt;text-align: left;">{{ part5 ? part5.line_4.value: '' }}</p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:144pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 3pt;text-indent: 0pt;line-height: 9pt;text-align: left;">5. ETHYLENE GAS</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p2_5b_EthyleneGasProof" style="text-indent: 0pt;text-align: left;">{{ part2 ? part2.ethyleneGas: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 2pt;padding-left: 2pt;text-indent: 0pt;line-height: 8pt;text-align: left;">5.<div id="p3_5a_Kind"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p3_5b_ProofNewCooperage" readonly=readonly /></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p3_5c_ProofUsedCooperage" readonly=readonly /></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p3_5d_ProofDepositedInTanks" readonly=readonly /></p>
            </td>
            <td style="width:198pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="3">
                <p class="s4" style="padding-left: 1pt;text-indent: 0pt;text-align: left;">5.<div id="p4_5a_Kind"></div></p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p4_5b_Proof" readonly=readonly /></p>
            </td>
            <td style="width:108pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" v-if="productionRecords" id="p5_5a_KindOfSpirits" style="padding-top: 1pt;padding-left: 2pt;text-indent: 0pt;text-align: left;">5.{{ part5 ? part5.line_5.spirit: '' }}</p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p v-if="productionRecords" id="p5_5b_Proof" style="text-indent: 0pt;text-align: left;">{{ part5 ? part5.line_5.value: '' }}</p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:144pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 3pt;text-indent: 0pt;line-height: 9pt;text-align: left;">6. SULPHITE LIQUORS</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p2_6b_SulphiteLiquorsProof" style="text-indent: 0pt;text-align: left;">{{ part2 ? part2.sulphiteLiquors: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 2pt;padding-left: 2pt;text-indent: 0pt;line-height: 8pt;text-align: left;">6.<div id="p3_6a_Kind"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p3_6b_ProofNewCooperage" readonly=readonly /></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p3_6c_ProofUsedCooperage" readonly=readonly /></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p3_6d_ProofDepositedInTanks" readonly=readonly /></p>
            </td>
            <td style="width:198pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="3">
                <p class="s4" style="padding-left: 1pt;text-indent: 0pt;text-align: left;">6.<div id="p4_6a_Kind"></div></p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p4_6b_Proof" readonly=readonly /></p>
            </td>
            <td style="width:108pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" v-if="productionRecords" id="p5_6a_KindOfSpirits" style="padding-top: 1pt;padding-left: 2pt;text-indent: 0pt;text-align: left;">6.{{ part5 ? part5.line_6.spirit: '' }}</p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p v-if="productionRecords" id="p5_6b_Proof" style="text-indent: 0pt;text-align: left;">{{ part5 ? part5.line_6.value: '' }}</p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:144pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 3pt;text-indent: 0pt;line-height: 9pt;text-align: left;">7. FROM REDISTILLATION</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p2_7b_FromRedistillationProof" style="text-indent: 0pt;text-align: left;">{{ part2 ? part2.fromRedistillation: '' }}</p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 2pt;padding-left: 2pt;text-indent: 0pt;line-height: 8pt;text-align: left;">7.<div id="p3_7a_Kind"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p3_7b_ProofNewCooperage" readonly=readonly /></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p3_7c_ProofUsedCooperage" readonly=readonly /></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p3_7d_ProofDepositedInTanks" readonly=readonly /></p>
            </td>
            <td style="width:198pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="3">
                <p class="s4" style="padding-left: 1pt;text-indent: 0pt;text-align: left;">7.<div id="p4_7a_Kind"></div></p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p4_7b_Proof" readonly=readonly /></p>
            </td>
            <td style="width:108pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" v-if="productionRecords" id="p5_7a_KindOfSpirits" style="padding-top: 1pt;padding-left: 2pt;text-indent: 0pt;text-align: left;">7.{{ part5 ? part5.line_7.spirit: '' }}</p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p v-if="productionRecords" id="p5_7b_Proof" style="text-indent: 0pt;text-align: left;">{{ part5 ? part5.line_7.value: '' }}</p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:144pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 3pt;text-indent: 0pt;text-align: left;">8.</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p2_8b_Proof" readonly=readonly /></p>
            </td>
            <td style="width:84pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 2pt;text-indent: 0pt;line-height: 9pt;text-align: left;">8.<div id="p3_8a_Kind"></div></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p3_8b_ProofNewCooperage" readonly=readonly /></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p3_8c_ProofUsedCooperage" readonly=readonly /></p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p3_8d_ProofDepositedInTanks" readonly=readonly /></p>
            </td>
            <td style="width:198pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="3">
                <p class="s4" style="padding-left: 1pt;text-indent: 0pt;text-align: left;">8.<div id="p4_8a_Kind"></div></p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p4_8b_Proof" readonly=readonly /></p>
            </td>
            <td style="width:108pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" v-if="productionRecords" id="p5_8a_KindOfSpirits" style="padding-left: 2pt;text-indent: 0pt;text-align: left;">8.{{ part5 ? part5.line_8.spirit: '' }}</p>
            </td>
            <td style="width:102pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p v-if="productionRecords" id="p5_8b_Proof" style="text-indent: 0pt;text-align: left;">{{ part5 ? part5.line_8.value: '' }}</p>
            </td>
        </tr>
    </table>
    <p class="s15" style="padding-top: 5pt;padding-left: 25pt;text-indent: -3pt;text-align: left;">1 <span class="s16">Does not include spirits withdrawn for experimental or research use under 26 U.S.C. 5312; report these at line 12.</span></p>
    <p class="s15" style="padding-left: 22pt;text-indent: 0pt;text-align: left;">2 <span class="s16">Show withdrawals under 26 U.S.C. 5214(a)(10), 5373(b)(4) , or 5312.</span></p>
    <p class="s15" style="padding-top: 5pt;padding-left: 22pt;text-indent: 0pt;text-align: left;">3 <span class="s16">Does not include recovered denatured spirits or articles returned for restoration and/or denaturation under</span></p>
    <p class="s16" style="padding-left: 25pt;text-indent: 0pt;text-align: left;">27 CFR 19.683; such materials should be reported on TTB F 5110.43, Monthly Report of Processing (Denaturing) Operations. Imported spirits received for redistillation will be identified as such and reported separately in Parts II and V .</p>
    <p class="s15" style="padding-top: 4pt;padding-left: 22pt;text-indent: 0pt;line-height: 8pt;text-align: left;">4 <span class="s16">Samples returned must be shown here.</span></p>
    <p class="s16" style="padding-left: 25pt;text-indent: -3pt;text-align: left;"><span class="s15">5 </span>If alcohol or spirits, indicate whether &quot;190 <span class="s17"></span><span class="s18"> </span>or over&quot; or &quot;under 190<span class="s17"></span>&quot;.</p>
    <p style="text-indent: 0pt;text-align: left;"><br /></p>
    <h1 style="text-indent: 0pt;line-height: 9pt;text-align: left;">TTB F 5110.40 <span class="s1">(01/2010)</span></h1>
    <p style="text-indent: 0pt;text-align: left;" />
    <p style="text-indent: 0pt;text-align: left;"><span><img width="1465" height="16" alt="image" src="../../assets/images/reporting/production/Image_003.png" /></span></p>
    <table style="border-collapse:collapse;margin-left:5.41999pt" cellspacing="0" id="tablePart6">
        <tr>
            <td colspan="5">
                <p class="s20" style="padding-bottom: 1pt;text-align: center;">PART VI - MATERIALS USED</p>
            </td>
        </tr>
        <tr style="height:47pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
                <p class="s4" style="padding-left: 91pt;padding-right: 90pt;text-indent: 0pt;text-align: center;">KIND</p>
                <p class="s4" style="padding-left: 91pt;padding-right: 90pt;text-indent: 0pt;text-align: center;">(a)</p>
            </td>
            <td style="width:174pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" colspan="2">
                <p style="text-indent: 0pt;text-align: left;"><br /></p>
                <p class="s4" style="padding-left: 44pt;padding-right: 41pt;text-indent: 0pt;text-align: left;">USED IN PRODUCTION OF DISTILLED SPIRITS</p>
            </td>
            <td style="width:181pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20" colspan="2">
                <p class="s4" style="padding-left: 42pt;padding-right: 39pt;text-indent: 0pt;text-align: center;">USED IN MANUFACTURE OF SUBSTANCES OTHER</p>
                <p class="s4" style="padding-left: 25pt;padding-right: 22pt;text-indent: 0pt;text-align: center;">THAN DISTILLED SPIRITS, IN PROCESSES YIELDING DISTILLED</p>
                <p class="s4" style="padding-left: 25pt;padding-right: 22pt;text-indent: 0pt;line-height: 8pt;text-align: center;">SPIRITS AS A BY-PRODUCT</p>
            </td>
        </tr>
        <tr style="height:24pt">
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 24pt;padding-right: 23pt;text-indent: 0pt;text-align: center;">POUNDS</p>
                <p class="s4" style="padding-left: 24pt;padding-right: 23pt;text-indent: 0pt;text-align: center;">(b)</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 2pt;padding-left: 25pt;padding-right: 23pt;text-indent: 0pt;text-align: center;">GALLONS</p>
                <p class="s4" style="padding-left: 25pt;padding-right: 23pt;text-indent: 0pt;text-align: center;">(c)</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 2pt;padding-left: 9pt;padding-right: 8pt;text-indent: 0pt;text-align: center;">POUNDS</p>
                <p class="s4" style="padding-left: 9pt;padding-right: 8pt;text-indent: 0pt;text-align: center;">(d)</p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p class="s4" style="padding-top: 2pt;padding-left: 26pt;padding-right: 24pt;text-indent: 0pt;text-align: center;">GALLONS</p>
                <p class="s4" style="padding-left: 26pt;padding-right: 24pt;text-indent: 0pt;text-align: center;">(e)</p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-right: 40pt;text-indent: 0pt;text-align: right;">GRAIN AND GRAIN PRODUCTS</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p v-if="productionRecords" id="p6_1b_PoundsCornUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.corn.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p v-if="productionRecords" id="p6_1c_GallonsCornUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.corn.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_1d_PoundsCornSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20" rowspan="2">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_1e_GallonsCornSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p id="part6GrainRow1" class="s4" style="padding-top: 6pt;padding-left: 1pt;text-indent: 0pt;text-align: left;">1. CORN</p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p id="part6GrainRow2" class="s4" style="padding-top: 7pt;padding-left: 2pt;text-indent: 0pt;text-align: left;">2. RYE</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_2b_PoundsRyeUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.rye.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_2c_GallonsRyeUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.rye.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_2d_PoundsRyeSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_2e_GallonsRyeSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p id="part6GrainRow3" class="s4" style="padding-top: 7pt;padding-left: 1pt;text-indent: 0pt;text-align: left;">3. MALT</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_3b_PoundsMaltUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.malt.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_3c_GallonsMaltUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.malt.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_3d_PoundsMaltSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_3e_GallonsMaltSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p id="part6GrainRow4" class="s4" style="padding-top: 6pt;padding-left: 1pt;text-indent: 0pt;text-align: left;">4. WHEAT</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_4b_PoundsWheatUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.wheat.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_4c_GallonsWheatUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.wheat.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_4d_PoundsWheatSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_4e_GallonsWheatSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p id="part6GrainRow5" class="s4" style="padding-top: 6pt;padding-left: 1pt;text-indent: 0pt;text-align: left;">5. SORGHUM GRAIN</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_5b_PoundsSourhumGrainUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.sorghum.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_5c_GallonsSourhumGrainUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.sorghum.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_5d_PoundsSourhumGrainSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_5e_GallonsSourhumGrainSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p id="part6GrainRow6" class="s4" style="padding-top: 5pt;padding-left: 1pt;text-indent: 0pt;text-align: left;">6. BARLEY</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_6b_PoundsBarleyUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.barley.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_6c_GallonsBarleyUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.barley.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_6d_PoundsBarleySpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_6e_GallonsBarleySpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_7a_Kind" class="s21" style="padding-top: 6pt;text-indent: 0pt;text-align: left;">7.{{ part6 ? part6.grain.line_7.material: '' }}</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_7b_PoundsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.line_7.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_7c_GallonsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.line_7.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_7d_PoundsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_7e_GallonsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_8a_Kind" class="s4" style="padding-top: 8pt;padding-left: 1pt;text-indent: 0pt;text-align: left;">8.{{ part6 ? part6.grain.line_8.material: '' }}</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_8b_PoundsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.line_8.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_8c_GallonsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.grain.line_8.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_8d_PoundsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_8e_GallonsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-right: 42pt;text-indent: 0pt;text-align: right;">FRUIT AND FRUIT PRODUCTS</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p v-if="productionRecords" id="p6_9b_PoundsGrapeUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.fruit.grape.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p v-if="productionRecords" id="p6_9c_GallonsGrapeUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.fruit.grape.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p style="text-indent: 0pt;text-align: left;"><div id="p6_9d_PoundsGrapeSpiritsAsByProduct"></div></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20" rowspan="2">
                <p style="text-indent: 0pt;text-align: left;"><div id="p6_9e_GallonsGrapeSpiritsAsByProduct"></div></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p id="part6FruitRow9" class="s4" style="padding-top: 6pt;padding-left: 1pt;text-indent: 0pt;text-align: left;">9. GRAPE</p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_10a_Kind" class="s4" style="padding-top: 8pt;padding-left: 2pt;text-indent: 0pt;text-align: left;">10.{{ part6 ? part6.fruit.line_10.material: '' }}</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_10b_PoundsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.fruit.line_10.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_10c_GallonsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.fruit.line_10.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_10d_PoundsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_10e_GallonsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_11a_Kind" class="s4" style="padding-top: 7pt;padding-left: 1pt;text-indent: 0pt;text-align: left;">11.{{ part6 ? part6.fruit.line_11.material: '' }}</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_11b_PoundsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.fruit.line_11.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_11c_GallonsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.fruit.line_11.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p6_11d_PoundsSpiritsAsByProduct"></div><input type="number" style="border:none" id="p6_11d_PoundsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_11e_GallonsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_12a_Kind" class="s4" style="padding-top: 6pt;padding-left: 1pt;text-indent: 0pt;text-align: left;">12.{{ part6 ? part6.fruit.line_12.material: '' }}</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_12b_PoundsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.fruit.line_12.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_12c_GallonsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.fruit.line_12.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_12d_PoundsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_12e_GallonsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:22pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_13a_Kind" class="s4" style="padding-top: 6pt;padding-left: 1pt;text-indent: 0pt;text-align: left;">13.{{ part6 ? part6.fruit.line_13.material: '' }}</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_13b_PoundsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.fruit.line_13.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_13c_GallonsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.fruit.line_13.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_13d_PoundsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_13e_GallonsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:24pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_14a_Kind" class="s4" style="padding-top: 7pt;text-indent: 0pt;text-align: left;">14.{{ part6 ? part6.fruit.line_14.material: '' }}</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_14b_PoundsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.fruit.line_14.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_14c_GallonsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.fruit.line_14.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_14d_PoundsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_14e_GallonsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-right: 45pt;text-indent: 0pt;text-align: right;">CANE AND CANE PRODUCTS</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p v-if="productionRecords" id="p6_15b_PoundsMolassesUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.cane.molasses.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p v-if="productionRecords" id="p6_15c_GallonsMolassesUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.cane.molasses.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p style="text-indent: 0pt;text-align: left;"><div id="p6_15d_PoundsMolassesSpiritsAsByProduct"></div></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20" rowspan="2">
                <p style="text-indent: 0pt;text-align: left;"><div id="p6_15e_GallonsMolassesSpiritsAsByProduct"></div></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 6pt;text-indent: 0pt;text-align: left;">15. MOLASSES</p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_16a_Kind" class="s4" style="padding-top: 7pt;text-indent: 0pt;text-align: left;">16.{{ part6 ? part6.cane.line_16.material: '' }}</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_16b_PoundsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.cane.line_16.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_16c_GallonsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.cane.line_16.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p6_16d_PoundsSpiritsAsByProduct"></div></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p6_16e_GallonsSpiritsAsByProduct"></div></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_17a_Kind" class="s4" style="padding-top: 6pt;text-indent: 0pt;text-align: left;">17.{{ part6 ? part6.cane.line_17.material: '' }}</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_17b_PoundsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.cane.line_17.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_17c_GallonsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.cane.line_17.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p6_17d_PoundsSpiritsAsByProduct"></div></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p6_17e_GallonsSpiritsAsByProduct"></div></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_18a_Kind" class="s4" style="padding-top: 7pt;text-indent: 0pt;text-align: left;">18.{{ part6 ? part6.cane.line_18.material: '' }}</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_18b_PoundsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.cane.line_18.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_18c_GallonsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.cane.line_18.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p6_18d_PoundsSpiritsAsByProduct"></div></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><div id="p6_18e_GallonsSpiritsAsByProduct"></div></p>
            </td>
        </tr>
        <tr style="height:11pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 1pt;padding-left: 63pt;text-indent: 0pt;text-align: left;">OTHER MATERIALS</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p v-if="productionRecords" id="p6_19b_PoundsEthylSulfateUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.other.ethyl_sulfate.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p v-if="productionRecords" id="p6_19c_GallonsEthylSulfateUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.other.ethyl_sulfate.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20" rowspan="2">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_19d_PoundsEthylSulfateSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20" rowspan="2">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_19e_GallonsEthylSulfateSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 6pt;text-indent: 0pt;text-align: left;">19. ETHYL SULFATE</p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 6pt;text-indent: 0pt;text-align: left;">20. ETHYLENE GAS</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_20b_PoundsEthyleneGasUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.other.ethylene_gas.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_20c_GallonsEthyleneGasUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.other.ethylene_gas.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_20d_PoundsEthyleneGasSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_20e_GallonsEthyleneGasSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 6pt;text-indent: 0pt;text-align: left;">21. SULPHITE LIQUORS</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_21b_PoundsSulphiteLiquorsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.other.sulphite_liquors.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_21c_GallonsSulphiteLiquorsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.other.sulphite_liquors.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_21d_PoundsSulphiteLiquorsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_21e_GallonsSulphiteLiquorsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-top: 7pt;text-indent: 0pt;text-align: left;">22. BUTANE GAS</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_22b_PoundsButaneGasUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.other.butane_gas.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_22c_GallonsButaneGasUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.other.butane_gas.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_22d_PoundsButaneGasSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_22e_GallonsButaneGasSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
        <tr style="height:23pt">
            <td style="width:203pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p  v-if="productionRecords" id="p6_23a_Kind" class="s4" style="padding-top: 6pt;text-indent: 0pt;text-align: left;">23.{{ part6 ? part6.other.line_23.material: '' }}</p>
            </td>
            <td style="width:85pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_23b_PoundsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.other.line_23.weight: '' }}</p>
            </td>
            <td style="width:89pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p v-if="productionRecords" id="p6_23c_GallonsUsedInProductionOfDistilledSpirits" style="text-indent: 0pt;text-align: left;">{{ part6 ? part6.other.line_23.volume: '' }}</p>
            </td>
            <td style="width:90pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_23d_PoundsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
            <td style="width:91pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p style="text-indent: 0pt;text-align: left;"><input type="number" style="border:none" id="p6_23e_GallonsSpiritsAsByProduct" readonly=readonly /></p>
            </td>
        </tr>
    </table>
    <table style="border-collapse:collapse;margin-left:5.42pt" cellspacing="0">
        <tr>
            <td colspan="3">
                <p class="s22" style="padding-top: 1pt;padding-left: 5pt;text-indent: 0pt;line-height: 115%;text-align: left;">Under penalties of perjury, I declare that I have examined this report, and to the best of my knowledge and belief, it is true, correct, and complete and is supported by true, correct, and complete commercial records which are available for inspection.</p>
            </td>
        </tr>
        <tr style="height:35pt">
            <td style="width:120pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="text-indent: 0pt;text-align: left;">DATE<div id="f_Date"></div></p>
            </td>
            <td style="width:216pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20;border-right-style:solid;border-right-width:1pt;border-right-color:#231F20">
                <p class="s4" style="padding-left: 1pt;text-indent: 0pt;text-align: left;">PROPRIETOR<div id="f_Proprietor"></div></p>
            </td>
            <td style="width:222pt;border-top-style:solid;border-top-width:1pt;border-top-color:#231F20;border-left-style:solid;border-left-width:1pt;border-left-color:#231F20;border-bottom-style:solid;border-bottom-width:1pt;border-bottom-color:#231F20">
                <p class="s4" style="padding-left: 1pt;text-indent: 0pt;text-align: left;">BY (Signature and title)<div id="f_SignatureAndTitle"></div></p>
            </td>
        </tr>
    </table>
    <p style="text-indent: 0pt;text-align: left;"><br /></p>
    <p class="s1" style="padding-left: 206pt;text-indent: 0pt;text-align: center;">INSTRUCTIONS</p>
    <p style="padding-top: 7pt;padding-left: 9pt;text-indent: 0pt;text-align: left;">Every proprietor of a distilled spirits plant must prepare this form monthly, in duplicate. Forward the original to the Director, National Revenue Center, 550 Main St, Ste 8002, Cincinnati, OH 45202-5215, not later than the 15th day of the month following that for which the report is rendered. The remaining copy must be retained by the proprietor.</p>
    <p style="text-indent: 0pt;text-align: left;"><br /></p>
    <h1 style="padding-top: 4pt;padding-left: 206pt;text-indent: 0pt;text-align: center;">PAPERWORK REDUCTION ACT NOTICE</h1>
    <p style="text-indent: 0pt;text-align: left;"><br /></p>
    <p style="padding-top: 5pt;padding-left: 7pt;text-indent: 0pt;text-align: left;">This information collection request is in accordance with the Paperwork Reduction Act of 1995. The purpose of this information collection is to monitor the operations of industries regulated by TTB. The information is required to verify unusual activities, errors, and omissions on taxable commodities. The information is mandatory by statue (26 U.S.C. 5207).</p>
    <p style="text-indent: 0pt;text-align: left;"><br /></p>
    <p style="padding-left: 8pt;text-indent: 0pt;text-align: left;">The estimated average burden associated with this collection of information is 2 hours per respondent or recordkeeper, depending on individual circumstances. Comments concerning the accuracy of this burden should be addressed to the Reports Management Officer, Regulations and Rulings Division, Alcohol and Tobacco Tax and Trade Bureau, Washington, D.C. 20220.</p>
    <p style="text-indent: 0pt;text-align: left;"><br /></p>
    <p style="padding-left: 8pt;text-indent: 0pt;text-align: left;">An agency may not conduct or sponsor, and a person is not required to respond to, a collection of information unless it displays a current, valid OMB control number.</p>
    <p style="text-indent: 0pt;text-align: left;"><br /></p>
    <p style="padding-left: 5pt;text-indent: 0pt;line-height: 1pt;text-align: left;"><span><img width="746" height="1" alt="image" src="../../assets/images/reporting/production/Image_005.png" /></span></p>
    <p class="s20" style="padding-top: 3pt;text-indent: 0pt;text-align: right;">TTB F 5110.40 (01/2010)</p>
  </div>
</template>


<script>
import { mapGetters } from 'vuex';
import dateHelper from '../../helpers/date-helper';

export default {
  name: 'ProductionReportView',
  computed: {
    productionRecords() {
      return this.$store.state.report.production;
    },
    ...mapGetters({ reportHeader: 'report/reportHeader' }),
    ...mapGetters({ whiskyUnder160: 'report/productionPart1WhiskyUnder160' }),
    ...mapGetters({ whiskyOver160: 'report/productionPart1WhiskyOver160' }),
    ...mapGetters({ brandyUnder170: 'report/productionPart1BrandyUnder170' }),
    ...mapGetters({ brandyOver170: 'report/productionPart1BrandyOver170' }),
    ...mapGetters({ rum: 'report/productionPart1Rum' }),
    ...mapGetters({ gin: 'report/productionPart1Gin' }),
    ...mapGetters({ vodka: 'report/productionPart1Vodka' }),
    ...mapGetters({ alcoholUnder190: 'report/productionPart1190AndOver' }),
    ...mapGetters({ alcoholOver190: 'report/productionPart1Under190' }),
    ...mapGetters({ other: 'report/productionPart1Other' }),
    ...mapGetters({ total: 'report/productionPart1Total' }),
    ...mapGetters({ part2: 'report/productionReportPart2'}),
    ...mapGetters({ part3: 'report/productionReportPart3'}),
    ...mapGetters({ part4: 'report/productionReportPart4'}),
    ...mapGetters({ part5: 'report/productionReportPart5'}),
    ...mapGetters({ part6: 'report/productionReportPart6'}),
  },
  methods: {
    getReport(startDate, endDate) {
      // get starting year
      const reportingYear = '2019';
      // let reportingYear = $("#ReportingYear").jqxDropDownList('getSelectedItem');

      // get starting month
      const reportingMonth = 'March';
      // let reportingMonth = $( "#ReportingMonth" ).jqxDropDownList('getSelectedItem');

      // converting to date object and getting starting/ending date
      const reportingDate = dateHelper.getReportingMonthAndYear(reportingYear, reportingMonth);
      // convert to UTC since all of the dates in the DB are stored in UTC
      const start = dateHelper.convertToUTC(reportingDate[0]);
      const end = dateHelper.convertToUTC(reportingDate[1]);
      this.$store.dispatch('report/getProduction', { start, end });
    },
  },
};
</script>

<style>
* {
    margin: 0;
    padding: 0;
    text-indent: 0;
}

.s1 {
    color: #231F20;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 8pt;
}

.s2 {
    color: #211E1E;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: bold;
    text-decoration: none;
    font-size: 8pt;
}

.s3 {
    color: #231F20;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: bold;
    text-decoration: none;
    font-size: 10pt;
}

.s4 {
    color: #231F20;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 8pt;
}

.s5 {
    color: black;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 8pt;
}

.s6 {
    color: #231F20;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: bold;
    text-decoration: none;
    font-size: 8pt;
}

.s7 {
    color: #231F20;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: bold;
    text-decoration: none;
    font-size: 12pt;
}

.s8 {
    color: #231F20;
    font-family: Arial, sans-serif;
    font-style: italic;
    font-weight: normal;
    text-decoration: none;
    font-size: 8pt;
}

h1 {
    color: #231F20;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: bold;
    text-decoration: none;
    font-size: 8pt;
}

.s9 {
    color: #231F20;
    font-family: Symbol, serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 8pt;
}

.s10 {
    color: #231F20;
    font-family: "Times New Roman", serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 8pt;
}

.s11 {
    color: #231F20;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 7pt;
}

.s12 {
    color: #231F20;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 4.5pt;
    vertical-align: 2pt;
}

.s13 {
    color: #231F20;
    font-family: Symbol, serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 9pt;
}

.s14 {
    color: #231F20;
    font-family: "Times New Roman", serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 9pt;
}

.s15 {
    color: #231F20;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 4pt;
    vertical-align: 2pt;
}

.s16 {
    color: #231F20;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 7pt;
}

.s17 {
    color: #231F20;
    font-family: Symbol, serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 7pt;
}

.s18 {
    color: #231F20;
    font-family: "Times New Roman", serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 7pt;
}

.s19 {
    color: #211E1E;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 8pt;
}

.s20 {
    color: black;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: bold;
    text-decoration: none;
    font-size: 8pt;
}

.s21 {
    color: black;
    font-family: Verdana, sans-serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 8pt;
}

.s22 {
    color: black;
    font-family: Arial, sans-serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 8pt;
}

p {
    color: black;
    font-family: Verdana, sans-serif;
    font-style: normal;
    font-weight: normal;
    text-decoration: none;
    font-size: 7.5pt;
    margin: 0pt;
}
</style>
