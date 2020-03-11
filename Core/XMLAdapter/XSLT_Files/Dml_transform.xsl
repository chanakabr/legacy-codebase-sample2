<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl" xmlns:ADIUtils="pda:ADIUtils" xmlns:offer="http://www.cablelabs.com/namespaces/metadata/xsd/offer/1" >
  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes"/>

  <!--<xsl:variable name="prefixURLPhysicalFile"><xsl:text>http://blink.cdn3.dmlib.com/</xsl:text></xsl:variable>-->
  <xsl:variable name="prefixURLPhysicalFile"></xsl:variable>
  <xsl:variable name="ImagePathConstVar"><xsl:text>http://cdn2.d3nw.com/</xsl:text></xsl:variable>

  <xsl:template match="/">
    <feed broadcasterName="whitelabel">
      <export>
        <xsl:apply-templates select="//*[local-name()='LibraryItem']/*[local-name()='BasicMetadata']/*[local-name()='WorkType'][. = 'Movie']"/>
        <xsl:apply-templates select="//*[local-name()='LibraryItem']/*[local-name()='BasicMetadata']/*[local-name()='WorkType'][. = 'Episode']"/>
        <xsl:apply-templates select="//*[local-name()='DeletedLibraryItemIds']"/>
      </export>
    </feed>
  </xsl:template>

  <xsl:template match="//*[local-name()='DeletedLibraryItemIds']">
    <!--handle delete-->
    <xsl:for-each select="*[local-name()='DeletedLibraryItemId']">
    <xsl:element name="media">
      <xsl:attribute name="is_active">false</xsl:attribute>
      <xsl:attribute name="co_guid">
        <xsl:value-of select="."/>
      </xsl:attribute>
      <xsl:attribute name="action">delete</xsl:attribute>
    </xsl:element>
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="//*[local-name()='LibraryItem']/*[local-name()='BasicMetadata']/*[local-name()='WorkType']">
    <!--handle episodes sesions or movies-->
    <xsl:choose>
      <xsl:when test=". = 'Movie'">
        <xsl:call-template name="handle_movies" />
      </xsl:when>
      <xsl:when test=". = 'Episode'">
        <xsl:call-template name="handle_episode" />
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="handle_episode">
      <xsl:element name="media">
        <xsl:attribute name="is_active">false</xsl:attribute>
        <xsl:attribute name="co_guid">
          <xsl:value-of select="../../*[local-name() = 'BasicMetadata']/@ContentID"/>
        </xsl:attribute>
        <xsl:attribute name="action">insert</xsl:attribute>
        <xsl:attribute name="erase">false</xsl:attribute>
        <xsl:call-template name="build_basic_data"/>
        <xsl:call-template name="build_episode_structure_data"/>
        <xsl:call-template name="build_files_data"/>
      </xsl:element>
  </xsl:template>

  <xsl:template name="handle_movies">
    <!--handle only the offsprings-->
    <xsl:if test="../../*[local-name() = 'DigitalAsset']">
      <xsl:element name="media">
        <xsl:attribute name="is_active">false</xsl:attribute>
        <xsl:attribute name="co_guid">
          <xsl:value-of select="../../*[local-name() = 'BasicMetadata']/@ContentID"/>
        </xsl:attribute>
        <xsl:attribute name="action">insert</xsl:attribute>
        <xsl:attribute name="erase">false</xsl:attribute>
        <xsl:call-template name="build_basic_data"/>
        <xsl:call-template name="build_movie_structure_data"/>
        <xsl:call-template name="build_files_data"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="build_files_data">
    <xsl:element name="files">
      <xsl:for-each select="../../*[local-name() = 'DigitalAsset']">
        <xsl:choose>
          
          <xsl:when test="./*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Drm']/*[local-name() = 'Type'][. = 'playready']">
            <xsl:element name="file">
              <xsl:attribute name="type"><xsl:text>Main</xsl:text></xsl:attribute>
              <xsl:attribute name="quality"><xsl:text>HIGH</xsl:text></xsl:attribute>
              <xsl:attribute name="pre_rule"><xsl:text>null</xsl:text></xsl:attribute>
              <xsl:attribute name="post_rule"><xsl:text>null</xsl:text></xsl:attribute>
              <xsl:attribute name="handling_type"><xsl:text>CLIP</xsl:text></xsl:attribute>
              <xsl:attribute name="duration"><xsl:value-of select="substring-before(substring-after(../*[local-name() = 'BasicMetadata']/*[local-name() = 'RunLength'],'PT'),'S')"/></xsl:attribute>
              <xsl:attribute name="cdn_name"><xsl:text>Smooth CDN</xsl:text></xsl:attribute>
              <xsl:attribute name="cdn_code">
                  <xsl:value-of select="concat($prefixURLPhysicalFile,./*[local-name() = 'FileInfo']/*[local-name() = 'Location'])" />
              </xsl:attribute>
              <xsl:attribute name="break_rule"><xsl:text>null</xsl:text></xsl:attribute>
              <xsl:attribute name="break_points"><xsl:text></xsl:text></xsl:attribute>
              <xsl:attribute name="billing_type"><xsl:text></xsl:text></xsl:attribute>
              <xsl:attribute name="assetwidth"><xsl:text></xsl:text></xsl:attribute>
              <xsl:attribute name="assetheight"><xsl:text></xsl:text></xsl:attribute>
              <xsl:attribute name="assetduration"><xsl:value-of select="substring-before(substring-after(../*[local-name() = 'BasicMetadata']/*[local-name() = 'RunLength'],'PT'),'S')"/></xsl:attribute>
              <xsl:attribute name="ppv_module"></xsl:attribute>
              <xsl:attribute name="co_guid">
                  <xsl:value-of select="./*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Drm']/*[local-name() = 'AssetId']" />
              </xsl:attribute>
            </xsl:element>
          </xsl:when>
          
          <xsl:when test="./*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Drm']/*[local-name() = 'Type'][. = 'widevine']">
            <xsl:choose>
              <xsl:when test="./*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Video']/*[local-name() = 'Picture']/*[local-name() = 'WidthPixels'] = 480">
                <xsl:element name="file">
                  <xsl:attribute name="type"><xsl:text>Smartphone Main</xsl:text></xsl:attribute>
                  <xsl:attribute name="quality"><xsl:text>HIGH</xsl:text></xsl:attribute>
                  <xsl:attribute name="pre_rule"><xsl:text>null</xsl:text></xsl:attribute>
                  <xsl:attribute name="post_rule"><xsl:text>null</xsl:text></xsl:attribute>
                  <xsl:attribute name="handling_type"><xsl:text>CLIP</xsl:text></xsl:attribute>
                  <xsl:attribute name="duration"><xsl:value-of select="substring-before(substring-after(../*[local-name() = 'BasicMetadata']/*[local-name() = 'RunLength'],'PT'),'S')"/></xsl:attribute>
                  <xsl:attribute name="cdn_name"><xsl:text>Widevine CDN</xsl:text></xsl:attribute>
                  <xsl:attribute name="cdn_code">
                      <xsl:value-of select="concat($prefixURLPhysicalFile,./*[local-name() = 'FileInfo']/*[local-name() = 'Location'])" />
                  </xsl:attribute>
                  <xsl:attribute name="break_rule"><xsl:text>null</xsl:text></xsl:attribute>
                  <xsl:attribute name="break_points"><xsl:text></xsl:text></xsl:attribute>
                  <xsl:attribute name="billing_type"><xsl:text></xsl:text></xsl:attribute>
                  <xsl:attribute name="assetwidth"><xsl:text></xsl:text></xsl:attribute>
                  <xsl:attribute name="assetheight"><xsl:text></xsl:text></xsl:attribute>
                  <xsl:attribute name="assetduration"><xsl:value-of select="substring-before(substring-after(../*[local-name() = 'BasicMetadata']/*[local-name() = 'RunLength'],'PT'),'S')"/></xsl:attribute>
                  <xsl:attribute name="ppv_module"><xsl:text></xsl:text></xsl:attribute>
                  <xsl:attribute name="co_guid">
                      <xsl:value-of select="./*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Drm']/*[local-name() = 'AssetId']" />
                  </xsl:attribute>
                </xsl:element>
              </xsl:when>

              <xsl:when test="./*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Video']/*[local-name() = 'Picture']/*[local-name() = 'WidthPixels'] = 640">
                <xsl:element name="file">
                  <xsl:attribute name="type"><xsl:text>Tablet Main</xsl:text></xsl:attribute>
                  <xsl:attribute name="quality"><xsl:text>HIGH</xsl:text></xsl:attribute>
                  <xsl:attribute name="pre_rule"><xsl:text>null</xsl:text></xsl:attribute>
                  <xsl:attribute name="post_rule"><xsl:text>null</xsl:text></xsl:attribute>
                  <xsl:attribute name="handling_type"><xsl:text>CLIP</xsl:text></xsl:attribute>
                  <xsl:attribute name="duration"><xsl:value-of select="substring-before(substring-after(../*[local-name() = 'BasicMetadata']/*[local-name() = 'RunLength'],'PT'),'S')"/></xsl:attribute>
                  <xsl:attribute name="cdn_name"><xsl:text>Widevine CDN</xsl:text></xsl:attribute>
                  <xsl:attribute name="cdn_code">
                      <xsl:value-of select="concat($prefixURLPhysicalFile,./*[local-name() = 'FileInfo']/*[local-name() = 'Location'])" />
                  </xsl:attribute>
                  <xsl:attribute name="break_rule"><xsl:text>null</xsl:text></xsl:attribute>
                  <xsl:attribute name="break_points"><xsl:text></xsl:text></xsl:attribute>
                  <xsl:attribute name="billing_type"><xsl:text></xsl:text></xsl:attribute>
                  <xsl:attribute name="assetwidth"><xsl:text></xsl:text></xsl:attribute>
                  <xsl:attribute name="assetheight"><xsl:text></xsl:text></xsl:attribute>
                  <xsl:attribute name="assetduration"><xsl:value-of select="substring-before(substring-after(../*[local-name() = 'BasicMetadata']/*[local-name() = 'RunLength'],'PT'),'S')"/></xsl:attribute>
                  <xsl:attribute name="ppv_module"><xsl:text></xsl:text></xsl:attribute>
                  <xsl:attribute name="co_guid">
                      <xsl:value-of select="./*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Drm']/*[local-name() = 'AssetId']" />
                  </xsl:attribute>
                </xsl:element>
              </xsl:when>
            </xsl:choose>
          </xsl:when>
          
        </xsl:choose>   
      </xsl:for-each>
      
      <xsl:call-template name="add_trailers"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="add_trailers">
    <xsl:variable name="contentID" select="../@ContentID" />
    <xsl:for-each select="//*[local-name() = 'LibraryItem']/*[local-name() = 'BasicMetadata']/*[local-name() = 'Parent']/*[local-name() = 'ParentContentID'][. = $contentID]">
      <xsl:if test="../../../*[local-name() = 'DigitalAsset']/*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Drm']/*[local-name() = 'Type'][. = 'playready']">
        <xsl:element name="file">
        <xsl:attribute name="type"><xsl:text>Trailer</xsl:text></xsl:attribute>
        <xsl:attribute name="quality"><xsl:text>HIGH</xsl:text></xsl:attribute>
        <xsl:attribute name="pre_rule"><xsl:text>null</xsl:text></xsl:attribute>
        <xsl:attribute name="post_rule"><xsl:text>null</xsl:text></xsl:attribute>
        <xsl:attribute name="handling_type"><xsl:text>CLIP</xsl:text></xsl:attribute>
        <xsl:attribute name="duration"><xsl:value-of select="substring-before(substring-after(../../../*[local-name() = 'BasicMetadata']/*[local-name() = 'RunLength'],'PT'),'S')"/></xsl:attribute>
        <xsl:attribute name="cdn_name"><xsl:text>Smooth CDN</xsl:text></xsl:attribute>
        <xsl:attribute name="cdn_code">
          <xsl:for-each select="../../../*[local-name() = 'DigitalAsset']/*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Drm']/*[local-name() = 'Type'][. = 'playready']" >
            <xsl:value-of select="concat($prefixURLPhysicalFile,../../../*[local-name() = 'FileInfo']/*[local-name() = 'Location'])" />
          </xsl:for-each>
        </xsl:attribute>
        <xsl:attribute name="break_rule"><xsl:text>null</xsl:text></xsl:attribute>
        <xsl:attribute name="break_points"><xsl:text></xsl:text></xsl:attribute>
        <xsl:attribute name="billing_type"><xsl:text></xsl:text></xsl:attribute>
        <xsl:attribute name="assetwidth"><xsl:text></xsl:text></xsl:attribute>
        <xsl:attribute name="assetheight"><xsl:text></xsl:text></xsl:attribute>
        <xsl:attribute name="assetduration"><xsl:value-of select="substring-before(substring-after(../../../*[local-name() = 'BasicMetadata']/*[local-name() = 'RunLength'],'PT'),'S')"/></xsl:attribute>
        <xsl:attribute name="ppv_module"></xsl:attribute>
        <xsl:attribute name="co_guid">
          <xsl:for-each select="../../../*[local-name() = 'DigitalAsset']/*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Drm']/*[local-name() = 'Type'][. = 'playready']" >
            <xsl:value-of select="../*[local-name() = 'AssetId']" />
          </xsl:for-each>
        </xsl:attribute>
      </xsl:element>
      </xsl:if>
      <xsl:if test="../../../*[local-name() = 'DigitalAsset']/*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Drm']/*[local-name() = 'Type'][. = 'widevine']">
        <xsl:element name="file">
          <xsl:choose>
            <xsl:when test="../../../*[local-name() = 'DigitalAsset']/*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Video']/*[local-name() = 'Picture']/*[local-name() = 'WidthPixels'] = 480">
              <xsl:attribute name="type"><xsl:text>Smartphone Trailer</xsl:text></xsl:attribute>
              <xsl:attribute name="quality"><xsl:text>HIGH</xsl:text></xsl:attribute>
              <xsl:attribute name="pre_rule"><xsl:text>null</xsl:text></xsl:attribute>
              <xsl:attribute name="post_rule"><xsl:text>null</xsl:text></xsl:attribute>
              <xsl:attribute name="handling_type"><xsl:text>CLIP</xsl:text></xsl:attribute>
              <xsl:attribute name="duration"><xsl:value-of select="substring-before(substring-after(../../../*[local-name() = 'BasicMetadata']/*[local-name() = 'RunLength'],'PT'),'S')"/></xsl:attribute>
              <xsl:attribute name="cdn_name"><xsl:text>Widevine CDN</xsl:text></xsl:attribute>
              <xsl:attribute name="cdn_code">
                <xsl:for-each select="../../../*[local-name() = 'DigitalAsset']/*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Drm']/*[local-name() = 'Type'][. = 'widevine']" >
                  <xsl:value-of select="concat($prefixURLPhysicalFile,../../../*[local-name() = 'FileInfo']/*[local-name() = 'Location'])" />
                </xsl:for-each>
              </xsl:attribute>
              <xsl:attribute name="break_rule"><xsl:text>null</xsl:text></xsl:attribute>
              <xsl:attribute name="break_points"><xsl:text></xsl:text></xsl:attribute>
              <xsl:attribute name="billing_type"><xsl:text></xsl:text></xsl:attribute>
              <xsl:attribute name="assetwidth"><xsl:text></xsl:text></xsl:attribute>
              <xsl:attribute name="assetheight"><xsl:text></xsl:text></xsl:attribute>
              <xsl:attribute name="assetduration"><xsl:value-of select="substring-before(substring-after(../../../*[local-name() = 'BasicMetadata']/*[local-name() = 'RunLength'],'PT'),'S')"/></xsl:attribute>
              <xsl:attribute name="ppv_module"><xsl:text></xsl:text></xsl:attribute>
              <xsl:attribute name="co_guid">
                <xsl:for-each select="../../../*[local-name() = 'DigitalAsset']/*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Drm']/*[local-name() = 'Type'][. = 'widevine']" >
                  <xsl:value-of select="../*[local-name() = 'AssetId']" />
                </xsl:for-each>
              </xsl:attribute>
            </xsl:when>
            <xsl:when test="../../../*[local-name() = 'DigitalAsset']/*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Video']/*[local-name() = 'Picture']/*[local-name() = 'WidthPixels'] = 640">
              <xsl:attribute name="type"><xsl:text>Tablet Trailer</xsl:text></xsl:attribute>
              <xsl:attribute name="quality"><xsl:text>HIGH</xsl:text></xsl:attribute>
              <xsl:attribute name="pre_rule"><xsl:text>null</xsl:text></xsl:attribute>
              <xsl:attribute name="post_rule"><xsl:text>null</xsl:text></xsl:attribute>
              <xsl:attribute name="handling_type"><xsl:text>CLIP</xsl:text></xsl:attribute>
              <xsl:attribute name="duration"><xsl:value-of select="substring-before(substring-after(../../../*[local-name() = 'BasicMetadata']/*[local-name() = 'RunLength'],'PT'),'S')"/></xsl:attribute>
              <xsl:attribute name="cdn_name"><xsl:text>Widevine CDN</xsl:text></xsl:attribute>
              <xsl:attribute name="cdn_code">
                <xsl:for-each select="../../../*[local-name() = 'DigitalAsset']/*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Drm']/*[local-name() = 'Type'][. = 'widevine']" >
                  <xsl:value-of select="concat($prefixURLPhysicalFile,../../../*[local-name() = 'FileInfo']/*[local-name() = 'Location'])" />
                </xsl:for-each>
              </xsl:attribute>
              <xsl:attribute name="break_rule"><xsl:text>null</xsl:text></xsl:attribute>
              <xsl:attribute name="break_points"><xsl:text></xsl:text></xsl:attribute>
              <xsl:attribute name="billing_type"><xsl:text></xsl:text></xsl:attribute>
              <xsl:attribute name="assetwidth"><xsl:text></xsl:text></xsl:attribute>
              <xsl:attribute name="assetheight"><xsl:text></xsl:text></xsl:attribute>
              <xsl:attribute name="assetduration"><xsl:value-of select="substring-before(substring-after(../../../*[local-name() = 'BasicMetadata']/*[local-name() = 'RunLength'],'PT'),'S')"/></xsl:attribute>
              <xsl:attribute name="ppv_module"><xsl:text></xsl:text></xsl:attribute>
              <xsl:attribute name="co_guid">
                <xsl:for-each select="../../../*[local-name() = 'DigitalAsset']/*[local-name() = 'DigitalAssetMetadata']/*[local-name() = 'Drm']/*[local-name() = 'Type'][. = 'widevine']" >
                  <xsl:value-of select="../*[local-name() = 'AssetId']" />
                </xsl:for-each>
              </xsl:attribute>
            </xsl:when>
          </xsl:choose>
      </xsl:element>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="build_basic_data">
    <xsl:element name="basic">    
      <xsl:element name="name">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:apply-templates select="../*[local-name() = 'LocalizedInfo']/*[local-name() = 'TitleDisplay19']" />
        </xsl:element>
      </xsl:element>
      <xsl:element name="description">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:apply-templates select="../*[local-name() = 'LocalizedInfo']/*[local-name() = 'Summary4000']" />
        </xsl:element>
      </xsl:element>
      <xsl:element name="media_type"><xsl:value-of select="."/></xsl:element>
      <xsl:element name="rules">
        <xsl:element name="geo_block_rule">
          <xsl:text>Philippines only</xsl:text>
        </xsl:element>
        <xsl:element name="watch_per_rule">
          <xsl:text>Parent allowed</xsl:text>
        </xsl:element>
      </xsl:element>
      
      <xsl:if test="../*[local-name() = 'LocalizedInfo']/*[local-name() = 'ArtReference']">
        <xsl:variable name="urlPostfix" select="../*[local-name() = 'LocalizedInfo']/*[local-name() = 'ArtReference']"/>
        <xsl:element name="thumb">
          <xsl:attribute name="url"><xsl:value-of select="concat($ImagePathConstVar,$urlPostfix)"/></xsl:attribute>
        </xsl:element>
        <xsl:element name="pic_ratios">
          <xsl:for-each select="../*[local-name() = 'LocalizedInfo']/*[local-name() = 'ArtReference']">
            <xsl:variable name="imageRatio" select="ADIUtils:GetRatio(substring-before(@resolution,'x'),substring-after(@resolution,'x'))" />
            <xsl:element name="ratio">
              <xsl:attribute name="thumb"><xsl:value-of select="concat($ImagePathConstVar,.)"/></xsl:attribute>
              <xsl:attribute name="ratio"><xsl:value-of select="ADIUtils:GetRatio(substring-before(@resolution,'x'),substring-after(@resolution,'x'))"/></xsl:attribute>
            </xsl:element>
          </xsl:for-each>
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_movie_structure_data">
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

  <xsl:template name="build_episode_structure_data">
    <xsl:element name="structure">
      <xsl:element name="strings">
        <xsl:call-template name="build_episode_strings_data"/>
      </xsl:element>
      <xsl:element name="doubles">
        <xsl:call-template name="build_episode_doubles_data"/>
      </xsl:element>
      <xsl:element name="booleans">
        <xsl:call-template name="build_booleans_data"/>
      </xsl:element>
      <xsl:element name="metas">
        <xsl:call-template name="build_episode_tags_data"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_episode_strings_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Long name</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="value">
        <xsl:attribute name="lang">eng</xsl:attribute>
        <xsl:value-of select="../*[local-name() = 'LocalizedInfo']/*[local-name() = 'TitleDisplayUnlimited']" />
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Summary short</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="value">
        <xsl:attribute name="lang">eng</xsl:attribute>
        <xsl:value-of select="../*[local-name() = 'LocalizedInfo']/*[local-name() = 'Summary190']" />
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Runtime</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="value">
        <xsl:attribute name="lang">eng</xsl:attribute>
        <xsl:value-of select="substring-before(substring-after(../*[local-name() = 'RunLength'],'T'),'S')" />
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Episode name</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="value">
        <xsl:attribute name="lang">eng</xsl:attribute>
        <xsl:value-of select="../*[local-name() = 'LocalizedInfo']/*[local-name() = 'TitleDisplay19']" />
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_episode_doubles_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Release year</xsl:attribute>
      <xsl:value-of select="../*[local-name() = 'ReleaseYear']" />
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Episode number</xsl:attribute>
      <xsl:value-of select="../*[local-name() = 'SequenceInfo']/*[local-name() = 'Number']" />
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Season number</xsl:attribute>
        <xsl:for-each select="../*[local-name() = 'AltIdentifier']/*[local-name() = 'Namespace'][. = 'Season_Number']">
          <xsl:value-of select="../*[local-name() = 'Identifier']"/>
        </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_strings_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Long name</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="value">
        <xsl:attribute name="lang">eng</xsl:attribute>
        <xsl:value-of select="../*[local-name() = 'LocalizedInfo']/*[local-name() = 'TitleDisplayUnlimited']" />
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Summary short</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="value">
        <xsl:attribute name="lang">eng</xsl:attribute>
        <xsl:value-of select="../*[local-name() = 'LocalizedInfo']/*[local-name() = 'Summary190']" />
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Runtime</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="value">
        <xsl:attribute name="lang">eng</xsl:attribute>
        <xsl:value-of select="substring-before(substring-after(../*[local-name() = 'RunLength'],'T'),'S')" />
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_doubles_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Release year</xsl:attribute>
        <xsl:value-of select="../*[local-name() = 'ReleaseYear']" />
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_booleans_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Cloased captions available</xsl:attribute>
        <xsl:choose>
          <xsl:when test="../*[local-name() = 'LocalizedInfo']/*[local-name() = 'DisplayIndicators'] = 'CC'">true</xsl:when>
          <xsl:otherwise>false</xsl:otherwise>
        </xsl:choose>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_episode_tags_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Genre</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:value-of select="../*[local-name() = 'LocalizedInfo']/*[local-name() = 'Genre']"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Rating</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:value-of select="../*[local-name() = 'RatingSet']/*[local-name() = 'Rating']/*[local-name() = 'Value']"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Country</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:value-of select="../*[local-name() = 'CountryOfOrigin']/*[local-name() = 'country']"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Main cast</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:for-each select="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'BillingBlockOrder'][. = '1']">
            <xsl:value-of select="../../*[local-name() = 'Name']/*[local-name() = 'DisplayName']"/>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:if test="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'BillingBlockOrder'][. = '2']">
            <xsl:for-each select="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'BillingBlockOrder'][. = '2']">
              <xsl:value-of select="../../*[local-name() = 'Name']/*[local-name() = 'DisplayName']"/>
            </xsl:for-each>
          </xsl:if>
        </xsl:element>
      </xsl:element>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:if test="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'BillingBlockOrder'][. = '3']">
            <xsl:for-each select="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'BillingBlockOrder'][. = '3']">
              <xsl:value-of select="../../*[local-name() = 'Name']/*[local-name() = 'DisplayName']"/>
            </xsl:for-each>
          </xsl:if>
        </xsl:element>
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Cast</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:apply-templates select="../*[local-name() = 'People']/*[local-name() = 'Name']" />
    </xsl:element>
    <xsl:if test="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'JobFunction'][. = 'Director']">
      <xsl:element name="meta">
        <xsl:attribute name="name">Director</xsl:attribute>
        <xsl:attribute name="ml_handling">unique</xsl:attribute>
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'JobFunction'][. = 'Director']">
              <xsl:value-of select="../../*[local-name() = 'Name']/*[local-name() = 'DisplayName']"/>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
    <xsl:if test="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'JobFunction'][. = 'Producer']">
      <xsl:element name="meta">
        <xsl:attribute name="name">Producer</xsl:attribute>
        <xsl:attribute name="ml_handling">unique</xsl:attribute>
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'JobFunction'][. = 'Producer']">
              <xsl:value-of select="../../*[local-name() = 'Name']/*[local-name() = 'DisplayName']"/>
            </xsl:for-each>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
    <xsl:element name="meta">
      <xsl:attribute name="name">Series name</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:variable name="firstParentKey" select="../*[local-name() = 'Parent']/*[local-name() = 'ParentContentID']"/>
          <xsl:for-each select="//*[local-name() = 'BasicMetadata'][@ContentID = $firstParentKey]">
            <xsl:variable name="secondParentKey" select="*[local-name() = 'Parent']/*[local-name() = 'ParentContentID']"/>
            <xsl:for-each select="//*[local-name() = 'BasicMetadata'][@ContentID = $secondParentKey]">
              <xsl:value-of select="*[local-name() = 'LocalizedInfo']/*[local-name() = 'TitleDisplay19']"/>
            </xsl:for-each>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="build_tags_data">
    <xsl:element name="meta">
      <xsl:attribute name="name">Genre</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:value-of select="../*[local-name() = 'LocalizedInfo']/*[local-name() = 'Genre']"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Rating</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:value-of select="../*[local-name() = 'RatingSet']/*[local-name() = 'Rating']/*[local-name() = 'Value']"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Country</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:value-of select="../*[local-name() = 'CountryOfOrigin']/*[local-name() = 'country']"/>
        </xsl:element>
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Main cast</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:for-each select="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'BillingBlockOrder'][. = '1']">
            <xsl:value-of select="../../*[local-name() = 'Name']/*[local-name() = 'DisplayName']"/>
          </xsl:for-each>
        </xsl:element>
      </xsl:element>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:if test="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'BillingBlockOrder'][. = '2']">
            <xsl:for-each select="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'BillingBlockOrder'][. = '2']">
              <xsl:value-of select="../../*[local-name() = 'Name']/*[local-name() = 'DisplayName']"/>
            </xsl:for-each>
          </xsl:if>
        </xsl:element>
      </xsl:element>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
          <xsl:if test="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'BillingBlockOrder'][. = '3']">
            <xsl:for-each select="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'BillingBlockOrder'][. = '3']">
              <xsl:value-of select="../../*[local-name() = 'Name']/*[local-name() = 'DisplayName']"/>
            </xsl:for-each>
          </xsl:if>
        </xsl:element>
      </xsl:element>
    </xsl:element>
    <xsl:element name="meta">
      <xsl:attribute name="name">Cast</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:apply-templates select="../*[local-name() = 'People']/*[local-name() = 'Name']" />
    </xsl:element>
    <xsl:if test="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'JobFunction'][. = 'Director']">
      <xsl:element name="meta">
      <xsl:attribute name="name">Director</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
        <xsl:element name="container">
          <xsl:element name="value">
            <xsl:attribute name="lang">eng</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'JobFunction'][. = 'Director']">
              <xsl:value-of select="../../*[local-name() = 'Name']/*[local-name() = 'DisplayName']"/>
            </xsl:for-each>
        </xsl:element>
      </xsl:element>
    </xsl:element>
    </xsl:if>
    <xsl:if test="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'JobFunction'][. = 'Producer']">
      <xsl:element name="meta">
      <xsl:attribute name="name">Producer</xsl:attribute>
      <xsl:attribute name="ml_handling">unique</xsl:attribute>
      <xsl:element name="container">
        <xsl:element name="value">
          <xsl:attribute name="lang">eng</xsl:attribute>
            <xsl:for-each select="../*[local-name() = 'People']/*[local-name() = 'Job']/*[local-name() = 'JobFunction'][. = 'Producer']">
              <xsl:value-of select="../../*[local-name() = 'Name']/*[local-name() = 'DisplayName']"/>
            </xsl:for-each>
        </xsl:element>
      </xsl:element>
    </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template match="//*[local-name() = 'People']/*[local-name() = 'Name']">
    <xsl:element name="container">
      <xsl:element name="value">
        <xsl:attribute name="lang">eng</xsl:attribute>
        <xsl:value-of select="*[local-name() = 'DisplayName']"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  
</xsl:stylesheet>