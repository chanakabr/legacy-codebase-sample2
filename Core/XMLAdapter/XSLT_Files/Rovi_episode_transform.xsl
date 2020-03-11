<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" xmlns:ADIUtils="pda:ADIUtils" xmlns:offer="http://www.cablelabs.com/namespaces/metadata/xsd/offer/1" >
  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes"/>

  <xsl:variable name="prefixURLPhysicalFile">
    <xsl:text></xsl:text>
  </xsl:variable>
  <xsl:variable name="ImagePathConstVar">
    <xsl:text></xsl:text>
  </xsl:variable>

  <xsl:variable name="smallcase" select="'abcdefghijklmnopqrstuvwxyz'" />
  <xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />

  <xsl:template match="/">
    <feed broadcasterName="whitelabel">
      <export>
        <xsl:apply-templates select="//*[local-name() = 'ContentList']/*[local-name() = 'Content']"/>
      </export>
    </feed>
  </xsl:template>

  <xsl:template match="//*[local-name() = 'ContentList']/*[local-name() = 'Content']">
    <xsl:call-template name="handle_media"/>
  </xsl:template>

  <xsl:template name="handle_media">
    <xsl:element name="media">
      <xsl:variable name="isMediaVerifed" select="./*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'PresentationStatus']"/>      
      <xsl:choose>
        <xsl:when test="translate($isMediaVerifed, $uppercase, $smallcase) = 'deleted'">
          <xsl:attribute name="is_active">false</xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="is_active">true</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:attribute name="co_guid">
        <xsl:value-of select="./*[local-name() = 'SeriesGroup']/*[local-name() = 'EpisodeId']"/>
      </xsl:attribute>
      <xsl:attribute name="action">insert</xsl:attribute>
      <xsl:call-template name="build_basic_data"/>
      <xsl:call-template name="build_structure_data"/>
      <xsl:call-template name="build_files_data"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="Set_TrailerFile_Type">
    <xsl:choose>
      <xsl:when test="../*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'MediaType']/*[local-name() = 'ShortName'] = 'MP4'">
        <xsl:attribute name="type">MP4 Trailer</xsl:attribute>
      </xsl:when>
      <xsl:when test="../*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'MediaType']/*[local-name() = 'ShortName'] = 'M3U8'">
        <xsl:attribute name="type">HLS Trailer</xsl:attribute>
      </xsl:when>
      <xsl:when test="../*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'MediaType']/*[local-name() = 'ShortName'] = 'ISM'">
        <xsl:attribute name="type">ISM Trailer</xsl:attribute>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="Set_File_Type">
    <xsl:choose>
      <xsl:when test="./*[local-name() = 'MediaType']/*[local-name() = 'ShortName'] = 'M3U8'">
        <xsl:choose>
          <xsl:when test="./*[local-name() = 'DimensionList']/*[local-name() = 'Dimension'] = 'SD'">
            <xsl:choose>                           
              <xsl:when test="translate(./*[local-name() = 'MediaType']/*[local-name() = 'EncryptionType'], $uppercase, $smallcase) = 'playready,marlin'">              
                <xsl:attribute name="type">Mobile Devices Main SD</xsl:attribute>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="./*[local-name() = 'DimensionList']/*[local-name() = 'Dimension'] = 'HD'">
            <xsl:choose>            
              <xsl:when test="translate(./*[local-name() = 'MediaType']/*[local-name() = 'EncryptionType'], $uppercase, $smallcase) = 'playready,marlin'">              
                <xsl:attribute name="type">Mobile Devices Main HD</xsl:attribute>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="./*[local-name() = 'MediaType']/*[local-name() = 'ShortName'] = 'ISM'">
        <xsl:choose>
          <xsl:when test="./*[local-name() = 'DimensionList']/*[local-name() = 'Dimension'] = 'SD'">
            <xsl:choose>              
              <xsl:when test="./*[local-name() = 'MediaType']/*[local-name() = 'EncryptionType'] = 'PLAYREADY'">                
                <xsl:attribute name="type">PC Main SD</xsl:attribute>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          <xsl:when test="./*[local-name() = 'DimensionList']/*[local-name() = 'Dimension'] = 'HD'">
            <xsl:choose>            
              <xsl:when test="./*[local-name() = 'MediaType']/*[local-name() = 'EncryptionType'] = 'PLAYREADY'">               
                <xsl:attribute name="type">PC Main HD</xsl:attribute>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
        </xsl:choose>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="build_files_data">
    <xsl:element name="files">
      <xsl:for-each select="./*[local-name() = 'FormatList']/*[local-name() = 'Format']" >

        <xsl:element name="file">
          <xsl:call-template name="Set_File_Type"/>
          <xsl:attribute name="quality">HIGH</xsl:attribute>
          <xsl:attribute name="pre_rule">null</xsl:attribute>
          <xsl:attribute name="post_rule">null</xsl:attribute>
          <xsl:attribute name="handling_type">CLIP</xsl:attribute>
          <xsl:attribute name="duration"><xsl:value-of select="../../*[local-name() = 'RunTimeMinutes']"/></xsl:attribute>
          <xsl:attribute name="cdn_name">Prefix URL VOD</xsl:attribute>
          <xsl:attribute name="cdn_code"><xsl:value-of select="./*[local-name() = 'StartUrl']"/></xsl:attribute>
          <xsl:attribute name="break_rule"></xsl:attribute>
          <xsl:attribute name="break_points"></xsl:attribute>
          <xsl:attribute name="billing_type">Tvinci</xsl:attribute>
          <xsl:attribute name="assetwidth"></xsl:attribute>
          <xsl:attribute name="assetheight"></xsl:attribute>
          <xsl:attribute name="assetduration"><xsl:value-of select="../../*[local-name() = 'RunTimeSeconds']"/></xsl:attribute>          
          <xsl:attribute  name="file_start_date">
            <xsl:call-template name="SetStartDateEpisode">
              <xsl:with-param name="contentIDEpisode" select ="../../*[local-name() = 'ContentId']"/>
            </xsl:call-template>
          </xsl:attribute>
          <xsl:attribute  name="file_end_date">
            <xsl:call-template name="SetEndDateEpisode">
              <xsl:with-param name="contentIDEpisode" select ="../../*[local-name() = 'ContentId']"/>
            </xsl:call-template>
          </xsl:attribute>          
          <xsl:attribute name="ppv_module">
            <xsl:variable name="contentID" select="../../*[local-name() = 'ContentId']"/>
            <xsl:for-each select="../../../../*[local-name() = 'LicenseList']/*[local-name() = 'License']">
              <xsl:if test="./*[local-name() = 'LicenseGrantType'] = 'EPISODE'">
              <xsl:for-each select="./*[local-name() = 'LicenseGrantsList']/*[local-name() = 'ContentIdList']/*[local-name() = 'ContentId']">
                <xsl:if test=". = $contentID">      
                  <xsl:variable name="LSfullTime" select="../../../*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodStart']"/>
                  <xsl:variable name="LSday" select="substring-after(substring-after(substring-before($LSfullTime,'T'),'-'),'-')"/>
                  <xsl:variable name="LSmonth" select="substring-before(substring-after(substring-before($LSfullTime,'T'),'-'),'-')"/>
                  <xsl:variable name="LSyear" select="substring-before($LSfullTime,'-')"/>
                  <xsl:variable name="LStime" select="substring-before(substring-after($LSfullTime,'T'),'Z')"/>
                  <xsl:variable name="LS" select="concat($LSday,'/',$LSmonth,'/',$LSyear,' ',$LStime)"/>

                  <xsl:variable name="LEfullTime" select="../../../*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodEnd']"/>   
                  <xsl:variable name="LEday" select="substring-after(substring-after(substring-before($LEfullTime,'T'),'-'),'-')"/>                  
                  <xsl:variable name="LEmonth" select="substring-before(substring-after(substring-before($LEfullTime,'T'),'-'),'-')"/>                  
                  <xsl:variable name="LEyear" select="substring-before($LEfullTime,'-')"/> 
                  <xsl:variable name="LEtime" select="substring-before(substring-after($LEfullTime,'T'),'Z')"/>
                  <xsl:variable name="LE" select="concat($LEday,'/',$LEmonth,'/',$LEyear,' ',$LEtime)"/>

                  <xsl:value-of select="concat(../../../*[local-name() = 'LicenseCode'],';',$LS,';',$LE,';')"/>
                </xsl:if>
              </xsl:for-each>
              </xsl:if> 
            </xsl:for-each>
          </xsl:attribute>
          <xsl:attribute name="co_guid"><xsl:value-of select="./*[local-name() = 'MediaType']/*[local-name() = 'EncryptionKeyId']"/></xsl:attribute>
          <xsl:attribute name="output_protection_level"><xsl:value-of select="./*[local-name() = 'MediaType']/*[local-name() = 'MinimumMediaOutputProtectionLevelGroup']/*[local-name() = 'Name']"/></xsl:attribute>
        </xsl:element>

      </xsl:for-each>

      <xsl:for-each select="//*[local-name() = 'PromotionContentList']/*[local-name() = 'PromotionContent']/*[local-name() = 'ContentType'][translate(., $uppercase, $smallcase) = 'trailer']" >
        <xsl:element name="file">
          <xsl:call-template name="Set_TrailerFile_Type"/>
          <xsl:attribute name="quality">HIGH</xsl:attribute>
          <xsl:attribute name="pre_rule">null</xsl:attribute>
          <xsl:attribute name="post_rule">null</xsl:attribute>
          <xsl:attribute name="handling_type">CLIP</xsl:attribute>
          <xsl:attribute name="duration">0</xsl:attribute>
          <xsl:attribute name="cdn_name">Default CDN</xsl:attribute>
          <xsl:attribute name="cdn_code"><xsl:value-of select="../*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'StartUrl']"/></xsl:attribute>
          <xsl:attribute name="break_rule"></xsl:attribute>
          <xsl:attribute name="break_points"></xsl:attribute>
          <xsl:attribute name="billing_type"></xsl:attribute>
          <xsl:attribute name="assetwidth"></xsl:attribute>
          <xsl:attribute name="assetheight"></xsl:attribute>
          <xsl:attribute name="assetduration">0</xsl:attribute>
          <xsl:attribute name="ppv_module"></xsl:attribute>
          <xsl:variable name="formatID" select="./*[local-name() = 'FormatId']"/>
          <xsl:variable name="formatIDU" select="concat($formatID,'_')"/>
          <xsl:variable name="contentID" select="../../*[local-name() = 'ContentId']"/>
          <xsl:attribute name="co_guid"><xsl:value-of select="concat($formatIDU,$contentID)"/></xsl:attribute>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_basic_data">
    <xsl:element name="basic">
      <xsl:element name="name">
          <xsl:element name="value">
          <xsl:attribute name="lang">
            <xsl:call-template name="Translate_Langueage"/>
          </xsl:attribute>
          <xsl:value-of select="./*[local-name() = 'ContentNameList']/*[local-name() = 'ContentName']"/>
          </xsl:element>
      </xsl:element>
      <xsl:element name="description">
          <xsl:element name="value">
          <xsl:attribute name="lang">
            <xsl:call-template name="Translate_Langueage"/>
          </xsl:attribute>
          <xsl:value-of select="./*[local-name() = 'SynopsisList']/*[local-name() = 'Synopsis']"/>
          </xsl:element>
      </xsl:element>
      <xsl:element name="media_type">
        <xsl:text>episode</xsl:text>
      </xsl:element>
      <xsl:element name="rules">
        <xsl:element name="geo_block_rule">
          <xsl:variable name="Number_Of_Countries" select="count(//*[local-name() = 'LicenseList']/License[1]/*[local-name() = 'LicenseGrantsList']/*[local-name() = 'TerritoryWhitelist']/*[local-name() = 'Territory'])"/>
          <xsl:choose>
            <xsl:when test="$Number_Of_Countries = 2">
              <xsl:text>Germany and Austria only</xsl:text>  
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="//*[local-name() = 'LicenseList']/License[1]/*[local-name() = 'LicenseGrantsList']/*[local-name() = 'TerritoryWhitelist']/Territory[1] = 'DE'">
                  <xsl:text>Germany Only</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>Austria Only</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:element>
        <xsl:element name="watch_per_rule">
          <xsl:text>Parent account allowed</xsl:text>
        </xsl:element>
      </xsl:element>
      <xsl:element name="dates">       
        <xsl:element name="start">
          <xsl:call-template name="SetStartDateEpisode">
            <xsl:with-param name="contentIDEpisode" select ="./*[local-name() = 'ContentId']"/>           
          </xsl:call-template>          
        </xsl:element>        
        <xsl:element name="final_end">
          <xsl:call-template name="SetEndDateEpisode">
            <xsl:with-param name="contentIDEpisode" select ="./*[local-name() = 'ContentId']"/>
          </xsl:call-template>
        </xsl:element>
        <xsl:element name="catalog_start">
          <xsl:call-template name="SetStartViewDate"/>
        </xsl:element>
        <xsl:element name="catalog_end">
          <xsl:call-template name="SetEndViewDate"/>
        </xsl:element>
      </xsl:element>
      
      <xsl:element name="thumb">
        <xsl:attribute name="url">
          <xsl:for-each select="//*[local-name() = 'PromotionContentList']/*[local-name() = 'PromotionContent']/*[local-name() = 'ContentTypeDisplay'][translate(., $uppercase, $smallcase) = 'cover']">
            <xsl:if test="../*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'AspectRatio'] = '2:3'">
              <xsl:value-of select="../*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'StartUrl']"/>
            </xsl:if>
            </xsl:for-each>
        </xsl:attribute>
      </xsl:element>

      <xsl:element name="pic_ratios">
        <xsl:element name="ratio">
          <xsl:attribute name="thumb">
            <xsl:for-each select="//*[local-name() = 'PromotionContentList']/*[local-name() = 'PromotionContent']/*[local-name() = 'ContentTypeDisplay'][translate(., $uppercase, $smallcase) = 'teaser']">
              <xsl:if test="../*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'AspectRatio'] = '16:9'">
                <xsl:value-of select="../*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'StartUrl']"/>
              </xsl:if>
            </xsl:for-each>
          </xsl:attribute>
          <xsl:attribute name="ratio">
            <xsl:text>16:9</xsl:text>
          </xsl:attribute>
        </xsl:element>
      </xsl:element>
      
    </xsl:element>
  </xsl:template>


  <xsl:template name="SetStartDateEpisode">
    <xsl:param name="index" select="1" />
    <xsl:param name="MostSooner" select="1" />
    <xsl:param name="total" select="count(//*[local-name() = 'LicenseList']/*[local-name() = 'License']) + 1" />
    <xsl:param name="wasInitialized" select ="0" />
    <xsl:param name="contentIDEpisode" select= "0"/>    
    <xsl:if test="not($index = $total)">
      <xsl:choose>
        <xsl:when test="//*[local-name() = 'LicenseList']/License[$index]/*[local-name() = 'LicenseGrantType'] = 'EPISODE'">
          <xsl:choose>
            <xsl:when test="//*[local-name() = 'LicenseList']/License[$index]/*[local-name() = 'LicenseGrantsList']/*[local-name() = 'ContentIdList']/*[local-name() = 'ContentId'] = $contentIDEpisode "> 
              <xsl:choose>                    
                <xsl:when test ="$wasInitialized = 0 ">  <!--if the MostSooner was not initialized yet, we take by default the value related to the specific episode-->                         
                  <xsl:call-template name="SetStartDateEpisode">
                    <xsl:with-param name="wasInitialized" select ="1" />
                    <xsl:with-param name="index" select="$index + 1" />
                    <xsl:with-param name="MostSooner" select="$index" />
                    <xsl:with-param name="contentIDEpisode" select ="$contentIDEpisode"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise>  <!--if the MostSooner was already intialized once, check to see if there is an earlier date matching to the same content ID--> 
                  <xsl:choose>    
                    <xsl:when test="//*[local-name() = 'LicenseList']/License[$index]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodStartUnix'] &lt; //*[local-name() = 'LicenseList']/License[$MostSooner]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodStartUnix']">         
                      <xsl:call-template name="SetStartDateEpisode">                           
                        <xsl:with-param name="index" select="$index + 1" />
                        <xsl:with-param name="MostSooner" select="$index" />
                        <xsl:with-param name="contentIDEpisode" select ="$contentIDEpisode"/>
                      </xsl:call-template>                      
                    </xsl:when>
                    <xsl:otherwise> <!--if the startDate is not earlier than the one held in MostSooner, continue-->
                      <xsl:call-template name="SetStartDateEpisode">
                        <xsl:with-param name="index" select="$index + 1" />
                        <xsl:with-param name="MostSooner" select="$MostSooner" />
                        <xsl:with-param name="contentIDEpisode" select ="$contentIDEpisode"/>
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose> 
                </xsl:otherwise>
              </xsl:choose>                 
            </xsl:when>
            <xsl:otherwise> <!--if the Licence Content ID does not match the one of Episode, continue -->
              <xsl:call-template name="SetStartDateEpisode">
                <xsl:with-param name="index" select="$index + 1" />
                <xsl:with-param name="MostSooner" select="$MostSooner" />
                <xsl:with-param name="contentIDEpisode" select ="$contentIDEpisode"/>
              </xsl:call-template>
            </xsl:otherwise>                     
          </xsl:choose>
        </xsl:when>       
        <xsl:otherwise>  <!--if the Licence is not of type 'Episode', continue-->  
          <xsl:call-template name="SetStartDateEpisode">
            <xsl:with-param name="index" select="$index + 1" />
            <xsl:with-param name="MostSooner" select="$MostSooner" />
            <xsl:with-param name="contentIDEpisode" select ="$contentIDEpisode"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>  
      </xsl:if>

    <xsl:if test="$index = $total">
      <xsl:variable name="fullTime" select="//*[local-name() = 'LicenseList']/License[$MostSooner]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodStart']"/>
      <xsl:variable name="day" select="substring-after(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
      <xsl:variable name="month" select="substring-before(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
      <xsl:variable name="year" select="substring-before($fullTime,'-')"/>
      <xsl:variable name="time" select="substring-before(substring-after($fullTime,'T'),'Z')"/>
      <xsl:variable name="with_day_slash" select="concat($day,'/')"/>
      <xsl:variable name="with_month_slash" select="concat($month,'/')"/>
      <xsl:variable name="day_month" select="concat($with_day_slash,$with_month_slash)"/>
      <xsl:variable name="day_month_year" select="concat($day_month,$year)"/>
      <xsl:variable name="day_month_year_s" select="concat($day_month_year,' ')"/>
      <xsl:value-of select="concat($day_month_year_s,$time)"/>
    </xsl:if>
  </xsl:template>



  
  <xsl:template name="SetEndDateEpisode">    
    <xsl:param name="index" select="1" />
    <xsl:param name="MostLatest" select="1" />
    <xsl:param name="total" select="count(//*[local-name() = 'LicenseList']/*[local-name() = 'License']) + 1" />
    <xsl:param name="wasInitialized" select = "0" />
    <xsl:param name="contentIDEpisode" select= "0"/>

    <xsl:if test="not($index = $total)">
      <xsl:choose>
        <xsl:when test="//*[local-name() = 'LicenseList']/License[$index]/*[local-name() = 'LicenseGrantType'] = 'EPISODE'">
          <xsl:choose>
            <xsl:when test="//*[local-name() = 'LicenseList']/License[$index]/*[local-name() = 'LicenseGrantsList']/*[local-name() = 'ContentIdList']/*[local-name() = 'ContentId'] = $contentIDEpisode ">
              <xsl:choose>
                <xsl:when test ="$wasInitialized = 0 ">
                  <!--if the $MostLatest was not initialized yet, we take by default the value related to the specific episode-->
                  <xsl:call-template name="SetEndDateEpisode">
                    <xsl:with-param name="wasInitialized" select ="1" />
                    <xsl:with-param name="index" select="$index + 1" />
                    <xsl:with-param name="MostLatest" select="$index" />
                    <xsl:with-param name="contentIDEpisode" select ="$contentIDEpisode"/>
                  </xsl:call-template>
                </xsl:when>
                <xsl:otherwise> <!--if the MostLatest was already intialized once, check to see if there is a later date matching to the same content ID-->
                  <xsl:choose>
                    <xsl:when test="//*[local-name() = 'LicenseList']/License[$index]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicenseUsagePeriodEndUnix'] &gt; //*[local-name() = 'LicenseList']/License[$MostLatest]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicenseUsagePeriodEndUnix']">
                      <xsl:call-template name="SetEndDateEpisode">
                        <xsl:with-param name="index" select="$index + 1" />
                        <xsl:with-param name="MostLatest" select="$index" />
                        <xsl:with-param name="contentIDEpisode" select ="$contentIDEpisode"/>
                      </xsl:call-template>
                    </xsl:when>
                    <xsl:otherwise> <!--if the endDate is not later than the one held in MostLatest, continue-->
                      <xsl:call-template name="SetEndDateEpisode">
                        <xsl:with-param name="index" select="$index + 1" />
                        <xsl:with-param name="MostLatest" select="$MostLatest" />
                        <xsl:with-param name="contentIDEpisode" select ="$contentIDEpisode"/>
                      </xsl:call-template>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise> <!--if the Licence Content ID does not match the one of Episode, continue -->
              <xsl:call-template name="SetEndDateEpisode">
                <xsl:with-param name="index" select="$index + 1" />
                <xsl:with-param name="MostLatest" select="$MostLatest" />
                <xsl:with-param name="contentIDEpisode" select ="$contentIDEpisode"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise> <!--if the Licence is not of type 'Episode', continue-->
          <xsl:call-template name="SetEndDateEpisode">
            <xsl:with-param name="index" select="$index + 1" />
            <xsl:with-param name="MostLatest" select="$MostLatest" />
            <xsl:with-param name="contentIDEpisode" select ="$contentIDEpisode"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

    <xsl:if test="$index = $total">
      <xsl:variable name="fullTime" select="//*[local-name() = 'LicenseList']/License[$MostLatest]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicenseUsagePeriodEnd']"/>
      <xsl:variable name="day" select="substring-after(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
      <xsl:variable name="month" select="substring-before(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
      <xsl:variable name="year" select="substring-before($fullTime,'-')"/>
      <xsl:variable name="time" select="substring-before(substring-after($fullTime,'T'),'Z')"/>
      <xsl:variable name="with_day_slash" select="concat($day,'/')"/>
      <xsl:variable name="with_month_slash" select="concat($month,'/')"/>
      <xsl:variable name="day_month" select="concat($with_day_slash,$with_month_slash)"/>
      <xsl:variable name="day_month_year" select="concat($day_month,$year)"/>
      <xsl:variable name="day_month_year_s" select="concat($day_month_year,' ')"/>
      <xsl:value-of select="concat($day_month_year_s,$time)"/>
    </xsl:if>
  </xsl:template>
  
 <!-- not used
  <xsl:template name="SetStartDate">
    <xsl:param name="index" select="1" />
    <xsl:param name="MostSooner" select="1" />
    <xsl:param name="total" select="count(./*[local-name() = 'LicenseList']/*[local-name() = 'License']) + 1" />

    <xsl:if test="not($index = $total)">
      <xsl:choose>
        <xsl:when test="./*[local-name() = 'LicenseList']/License[$index]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodStartUnix'] &lt; ./*[local-name() = 'LicenseList']/License[$MostSooner]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodStartUnix']">
          <xsl:call-template name="SetStartDate">
            <xsl:with-param name="index" select="$index + 1" />
            <xsl:with-param name="MostSooner" select="$index" />
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="SetStartDate">
            <xsl:with-param name="index" select="$index + 1" />
            <xsl:with-param name="MostSooner" select="$MostSooner" />
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

    <xsl:if test="$index = $total">
      <xsl:variable name="fullTime" select="./*[local-name() = 'LicenseList']/License[$MostSooner]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicensePurchasePeriodStart']"/>
      <xsl:variable name="day" select="substring-after(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
      <xsl:variable name="month" select="substring-before(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
      <xsl:variable name="year" select="substring-before($fullTime,'-')"/>
      <xsl:variable name="time" select="substring-before(substring-after($fullTime,'T'),'Z')"/>
      <xsl:variable name="with_day_slash" select="concat($day,'/')"/>
      <xsl:variable name="with_month_slash" select="concat($month,'/')"/>
      <xsl:variable name="day_month" select="concat($with_day_slash,$with_month_slash)"/>
      <xsl:variable name="day_month_year" select="concat($day_month,$year)"/>
      <xsl:variable name="day_month_year_s" select="concat($day_month_year,' ')"/>
      <xsl:value-of select="concat($day_month_year_s,$time)"/>
    </xsl:if>
  </xsl:template>
  
  
  <xsl:template name="SetEndDate">
    <xsl:param name="index" select="1" />
    <xsl:param name="MostLatest" select="1" />
    <xsl:param name="total" select="count(./*[local-name() = 'LicenseList']/*[local-name() = 'License']) + 1" />

    <xsl:if test="not($index = $total)">
      <xsl:choose>
        <xsl:when test="./*[local-name() = 'LicenseList']/License[$index]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicenseUsagePeriodEndUnix'] &gt; ./*[local-name() = 'LicenseList']/License[$MostLatest]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicenseUsagePeriodEndUnix']">
          <xsl:call-template name="SetEndDate">
            <xsl:with-param name="index" select="$index + 1" />
            <xsl:with-param name="MostLatest" select="$index" />
          </xsl:call-template>
        </xsl:when>
        
        <xsl:otherwise>
          <xsl:call-template name="SetEndDate">
            <xsl:with-param name="index" select="$index + 1" />
            <xsl:with-param name="MostLatest" select="$MostLatest" />
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

    <xsl:if test="$index = $total">
      <xsl:variable name="fullTime" select="//*[local-name() = 'LicenseList']/License[$MostLatest]/*[local-name() = 'LicensePeriodGroup']/*[local-name() = 'LicenseUsagePeriodEnd']"/>
      <xsl:variable name="day" select="substring-after(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
      <xsl:variable name="month" select="substring-before(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
      <xsl:variable name="year" select="substring-before($fullTime,'-')"/>
      <xsl:variable name="time" select="substring-before(substring-after($fullTime,'T'),'Z')"/>
      <xsl:variable name="with_day_slash" select="concat($day,'/')"/>
      <xsl:variable name="with_month_slash" select="concat($month,'/')"/>
      <xsl:variable name="day_month" select="concat($with_day_slash,$with_month_slash)"/>
      <xsl:variable name="day_month_year" select="concat($day_month,$year)"/>
      <xsl:variable name="day_month_year_s" select="concat($day_month_year,' ')"/>
      <xsl:value-of select="concat($day_month_year_s,$time)"/>
    </xsl:if>
  </xsl:template>
  -->
  
  <xsl:template name="SetStartViewDate">
    <xsl:if test="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'VisibilityPeriod']/*[local-name() = 'VisibilityPeriodStart']">
      <xsl:variable name="fullTime" select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'VisibilityPeriod']/*[local-name() = 'VisibilityPeriodStart']"/>
      <xsl:variable name="day" select="substring-after(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
      <xsl:variable name="month" select="substring-before(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
      <xsl:variable name="year" select="substring-before($fullTime,'-')"/>
      <xsl:variable name="time" select="substring-before(substring-after($fullTime,'T'),'Z')"/>
      <xsl:variable name="with_day_slash" select="concat($day,'/')"/>
      <xsl:variable name="with_month_slash" select="concat($month,'/')"/>
      <xsl:variable name="day_month" select="concat($with_day_slash,$with_month_slash)"/>
      <xsl:variable name="day_month_year" select="concat($day_month,$year)"/>
      <xsl:variable name="day_month_year_s" select="concat($day_month_year,' ')"/>
      <xsl:value-of select="concat($day_month_year_s,$time)"/>
    </xsl:if>
  </xsl:template>

  <xsl:template name="SetEndViewDate">
    <xsl:choose>
      <xsl:when test="not(//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'VisibilityPeriod']/*[local-name() = 'VisibilityPeriodEnd'] = '')">
        <xsl:variable name="fullTime" select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'VisibilityPeriod']/*[local-name() = 'VisibilityPeriodEnd']"/>
        <xsl:variable name="day" select="substring-after(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
        <xsl:variable name="month" select="substring-before(substring-after(substring-before($fullTime,'T'),'-'),'-')"/>
        <xsl:variable name="year" select="substring-before($fullTime,'-')"/>
        <xsl:variable name="time" select="substring-before(substring-after($fullTime,'T'),'Z')"/>
        <xsl:variable name="with_day_slash" select="concat($day,'/')"/>
        <xsl:variable name="with_month_slash" select="concat($month,'/')"/>
        <xsl:variable name="day_month" select="concat($with_day_slash,$with_month_slash)"/>
        <xsl:variable name="day_month_year" select="concat($day_month,$year)"/>
        <xsl:variable name="day_month_year_s" select="concat($day_month_year,' ')"/>
        <xsl:value-of select="concat($day_month_year_s,$time)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>1/1/2099 00:00:00</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="build_structure_data">
    <xsl:element name="structure">
      <xsl:element name="strings">
        <xsl:call-template name="build_strings_data"/>
      </xsl:element>
      <xsl:element name="doubles">
        <xsl:call-template name="build_doubles_data"/>
      </xsl:element>
      <xsl:element name="booleans">
        <xsl:call-template name="build_booleans_data"/>
      </xsl:element>
      <xsl:element name="metas">
        <xsl:call-template name="build_tags_data"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_strings_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Summary short</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
        <xsl:element name="value">
          <xsl:attribute name="lang">
            <xsl:call-template name="Translate_Langueage"/>
          </xsl:attribute>         
          <xsl:value-of select="./*[local-name() = 'SynopsisList']/*[local-name() = 'Synopsis']"/>
        </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Runtime</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="value">
        <xsl:attribute name="lang">
          <xsl:call-template name="Translate_Langueage"/>
        </xsl:attribute>
        <xsl:value-of select="./*[local-name() = 'RunTimeMinutes']"/>       
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Short title</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="value">
        <xsl:attribute name="lang">
          <xsl:call-template name="Translate_Langueage"/>
        </xsl:attribute>
        <xsl:value-of select="./*[local-name() = 'ContentNameList']/*[local-name() = 'ContentName']"/>
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Episode name</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="value">
        <xsl:attribute name="lang">
          <xsl:call-template name="Translate_Langueage"/>
        </xsl:attribute>      
        <xsl:value-of select="./*[local-name() = 'ContentNameList']/*[local-name() = 'ContentName']"/>
      </xsl:element>
    </xsl:element>
    <!--<xsl:element name="meta">
      <xsl:attribute name="name">Series ID</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="value">
        <xsl:attribute name="lang">
          <xsl:call-template name="Translate_Langueage"/>
        </xsl:attribute>
        <xsl:value-of select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'SeriesGroup']/*[local-name() = 'SeriesId']"/>
      </xsl:element>
    </xsl:element>-->
   
  </xsl:template>

  <xsl:template name="build_doubles_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Release year</xsl:attribute>
      <xsl:value-of select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'Production']/*[local-name() = 'ReleaseYear']"/>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Season number</xsl:attribute>
      <xsl:value-of select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'SeriesGroup']/*[local-name() = 'SeasonNo']"/>    
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Episode number</xsl:attribute>
      <xsl:value-of select ="./*[local-name() = 'SeriesGroup']/*[local-name() = 'EpisodeNo']"/>         
    </xsl:element>
    
  </xsl:template>

  <xsl:template name="build_booleans_data">
    <xsl:element name="meta">
      <xsl:attribute name="name"></xsl:attribute>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_tags_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Main cast</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'ActorNameList']/*[local-name() = 'ActorName']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">de</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Director</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'DirectorNameList']/*[local-name() = 'DirectorName']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">de</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Genre</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'GenresNameList']/*[local-name() = 'GenreName']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">de</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Dimension</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'DimensionList']/*[local-name() = 'Dimension']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">de</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Studio</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">de</xsl:attribute>
          <xsl:value-of select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'CopyrightDisplay']"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Provider</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">de</xsl:attribute>
            <xsl:value-of select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'ProviderList']/*[local-name() = 'Provider']/*[local-name() = 'ProviderId']"/>
          </xsl:element>
        </xsl:element>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Country</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:for-each select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'Production']/*[local-name() = 'ProductionCountryDisplay']">
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">de</xsl:attribute>
            <xsl:value-of select="."/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Audio language</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">de</xsl:attribute>
          <xsl:value-of select="./*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'AudioTrackList']/*[local-name() = 'AudioTrack']/*[local-name() = 'Language']"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Subtitle language</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">de</xsl:attribute>
          <xsl:value-of select="./*[local-name() = 'FormatList']/*[local-name() = 'Format']/*[local-name() = 'SubtitleTrackList']"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Parental Rating</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">de</xsl:attribute>
          <xsl:for-each select="./*[local-name() = 'ParentalControlList']/*[local-name() = 'ParentalControl']">
            <xsl:value-of select="./*[local-name() = 'ParentalControlId']"/>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>
    </xsl:element>

    <xsl:element name="meta">
      <xsl:attribute name="name">Series name</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">de</xsl:attribute>
          <xsl:value-of select="//*[local-name() = 'PresentationMetaGroup']/*[local-name() = 'Title']/*[local-name() = 'TitleNameList']/*[local-name() = 'SeriesTitleName']"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>

  </xsl:template>

  <xsl:template name="Translate_Langueage">
    <xsl:variable name="languageCode" select="./*[local-name() = 'ContentNameList']/*[local-name() = 'ContentNameLong']/@lang"/>
    <xsl:choose>
      <xsl:when test="$languageCode = 'de'">
        <xsl:text>de</xsl:text>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>